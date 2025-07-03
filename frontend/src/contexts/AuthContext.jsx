// src/contexts/AuthContext.jsx
import { createContext, useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import jwtDecode from 'jwt-decode';

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState(null);
  const navigate = useNavigate();

  // Check if token is expired
  const isTokenExpired = (token) => {
    try {
      const decoded = jwtDecode(token);
      return decoded.exp * 1000 < Date.now();
    } catch (error) {
      return true;
    }
  };

  // Load user from token
  const loadUser = useCallback(async () => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      setLoading(false);
      return;
    }
    
    // Check if token is expired
    if (isTokenExpired(token)) {
      localStorage.removeItem('token');
      setUser(null);
      setLoading(false);
      return;
    }
    
    try {
      const response = await api.get('/api/auth/validate');
      setUser(response.data.user);
    } catch (error) {
      console.error('Error validating token:', error);
      localStorage.removeItem('token');
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadUser();
  }, [loadUser]);

  const login = async (credentials) => {
    setAuthError(null);
    try {
      const response = await api.post('/api/auth/login', credentials);
      const { token, user } = response.data;
      
      localStorage.setItem('token', token);
      setUser(user);
      
      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      let message = 'Login failed';
      
      if (error.response) {
        // The request was made and the server responded with a status code
        // that falls out of the range of 2xx
        if (error.response.status === 401) {
          message = 'Invalid email or password';
        } else if (error.response.data && error.response.data.detail) {
          message = error.response.data.detail;
        }
      } else if (error.request) {
        // The request was made but no response was received
        message = 'Server not responding. Please try again later.';
      }
      
      setAuthError(message);
      return { success: false, message };
    }
  };

  const register = async (userData) => {
    setAuthError(null);
    try {
      const response = await api.post('/api/auth/register', userData);
      const { token, user } = response.data;
      
      localStorage.setItem('token', token);
      setUser(user);
      
      return { success: true };
    } catch (error) {
      console.error('Registration error:', error);
      let message = 'Registration failed';
      
      if (error.response && error.response.data && error.response.data.detail) {
        message = error.response.data.detail;
      } else if (error.response && error.response.status === 409) {
        message = 'A user with this email already exists';
      }
      
      setAuthError(message);
      return { success: false, message };
    }
  };

  const logout = async () => {
    try {
      // Optional: Call logout endpoint to invalidate token on server
      await api.post('/api/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('token');
      setUser(null);
      navigate('/login');
    }
  };

  // Refresh token if it's about to expire
  const refreshToken = async () => {
    try {
      const response = await api.post('/api/auth/refresh');
      const { token } = response.data;
      
      localStorage.setItem('token', token);
      return true;
    } catch (error) {
      console.error('Token refresh error:', error);
      return false;
    }
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      register, 
      logout, 
      loading, 
      authError,
      refreshToken
    }}>
      {children}
    </AuthContext.Provider>
  );
};