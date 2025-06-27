import React, { createContext, useState, useEffect } from 'react';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import { createAppTheme } from '../styles/muiTheme';
import axios from 'axios';

const ThemeContext = createContext();

export const ThemeProvider = ({ children }) => {
  const [theme, setTheme] = useState('light');
  const [isLoading, setIsLoading] = useState(true);

  // Create MUI theme based on current theme mode
  const muiTheme = createAppTheme(theme);

  // Load theme from server when user is authenticated
  useEffect(() => {
    const loadUserPreferences = async () => {
      try {
        const token = localStorage.getItem('token');
        if (token) {
          const response = await axios.get('http://localhost:8000/api/preferences');
          const serverTheme = response.data.theme || 'light';
          console.log('Loaded theme from server:', serverTheme);
          setTheme(serverTheme);
          localStorage.setItem('theme', serverTheme); // Keep local backup
          document.body.className = serverTheme;
        } else {
          // No token, use localStorage fallback
          const savedTheme = localStorage.getItem('theme') || 'light';
          console.log('Loaded theme from localStorage:', savedTheme);
          setTheme(savedTheme);
          document.body.className = savedTheme;
        }
      } catch (error) {
        console.error('Error loading user preferences:', error);
        // Fallback to localStorage
        const savedTheme = localStorage.getItem('theme') || 'light';
        setTheme(savedTheme);
        document.body.className = savedTheme;
      } finally {
        setIsLoading(false);
      }
    };

    // Load preferences on component mount
    loadUserPreferences();

    // Listen for auth events
    const handleUserLoggedIn = () => {
      console.log('User logged in, reloading theme preferences');
      loadUserPreferences();
    };

    const handleUserLoggedOut = () => {
      console.log('User logged out, clearing theme preferences');
      const defaultTheme = 'light';
      setTheme(defaultTheme);
      localStorage.setItem('theme', defaultTheme);
      document.body.className = defaultTheme;
    };

    window.addEventListener('userLoggedIn', handleUserLoggedIn);
    window.addEventListener('userLoggedOut', handleUserLoggedOut);

    return () => {
      window.removeEventListener('userLoggedIn', handleUserLoggedIn);
      window.removeEventListener('userLoggedOut', handleUserLoggedOut);
    };
  }, []);

  // Function to toggle the theme
  const toggleTheme = async () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    console.log('Toggling theme to:', newTheme);
    
    setTheme(newTheme);
    localStorage.setItem('theme', newTheme); // Keep local backup
    document.body.className = newTheme;

    // Sync with server if user is authenticated
    try {
      const token = localStorage.getItem('token');
      if (token) {
        await axios.put('http://localhost:8000/api/preferences', { theme: newTheme });
        console.log('Theme synced with server:', newTheme);
      }
    } catch (error) {
      console.error('Error syncing theme with server:', error);
      // Theme change still works locally even if server sync fails
    }
  };

  // Function to manually reload preferences (can be called from other components)
  const reloadThemePreferences = async () => {
    try {
      const token = localStorage.getItem('token');
      if (token) {
        const response = await axios.get('http://localhost:8000/api/preferences');
        const serverTheme = response.data.theme || 'light';
        console.log('Reloaded theme from server:', serverTheme);
        setTheme(serverTheme);
        localStorage.setItem('theme', serverTheme);
        document.body.className = serverTheme;
      }
    } catch (error) {
      console.error('Error reloading user preferences:', error);
    }
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme, isLoading, reloadThemePreferences }}>
      <MuiThemeProvider theme={muiTheme}>
        <CssBaseline />
        {children}
      </MuiThemeProvider>
    </ThemeContext.Provider>
  );
};

export default ThemeContext;