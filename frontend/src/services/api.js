// src/services/api.js
import axios from 'axios';
import jwtDecode from 'jwt-decode';

const api = axios.create({
  baseURL: process.env.NODE_ENV === 'production' 
    ? window.location.origin // Use same origin in production
    : 'http://localhost:8001', // Development URL
  withCredentials: true // Important for CSRF protection when using cookies
});

// Check if token is close to expiring (within 5 minutes)
const isTokenNearExpiration = (token) => {
  try {
    const decoded = jwtDecode(token);
    // Check if token expires in less than 5 minutes
    return decoded.exp * 1000 < Date.now() + 5 * 60 * 1000;
  } catch (error) {
    return false;
  }
};

// Add request interceptor to include auth token
api.interceptors.request.use(async (config) => {
  const token = localStorage.getItem('token');
  
  if (token) {
    // Check if token is close to expiration
    if (isTokenNearExpiration(token)) {
      try {
        // Try to refresh the token
        const response = await axios.post('/api/auth/refresh', {}, {
          baseURL: config.baseURL,
          withCredentials: true
        });
        
        // Update token in localStorage
        localStorage.setItem('token', response.data.token);
      } catch (error) {
        console.error('Failed to refresh token:', error);
        // If refresh fails, proceed with the original token
      }
    }
    
    // Get the (potentially refreshed) token
    const currentToken = localStorage.getItem('token');
    if (currentToken) {
      config.headers.Authorization = `Bearer ${currentToken}`;
    }
  }
  
  return config;
}, (error) => {
  return Promise.reject(error);
});

// Add response interceptor to handle authentication errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Clear token on 401 Unauthorized
      localStorage.removeItem('token');
      
      // Redirect to login page
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;