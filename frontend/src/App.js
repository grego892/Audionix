import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import Home from './pages/Home/Home.js';
import Studio from './pages/Studio/Studio.js';
import FileManager from "./pages/FileManager/FileManager";
import Setup from "./pages/Setup/Setup";
import Login from './pages/Login/Login';
import Register from './pages/Register/Register';
import NavigationDrawer from './components/layout/NavigationDrawer/NavigationDrawer.js';
import ProtectedRoute from './components/auth/ProtectedRoute';
import { AuthProvider } from './contexts/AuthContext';
import ThemeContext from './contexts/ThemeContext';

// Example of how to add admin route to your App.js
import UserManagement from './pages/Admin/UserManagement';

function App() {
  const { isLoading } = useContext(ThemeContext);

  // Show loading spinner while theme is being loaded
  if (isLoading) {
    return (
      <Box 
        sx={{ 
          display: 'flex', 
          justifyContent: 'center', 
          alignItems: 'center', 
          minHeight: '100vh',
          bgcolor: 'background.default' // Use theme colors
        }}
      >
        <CircularProgress />
      </Box>
    );
  }
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          {/* Protected routes */}
          <Route path="/*" element={
            <ProtectedRoute>
              <Box sx={{ 
                display: 'flex',
                bgcolor: 'background.default',
                minHeight: '100vh',
                color: 'text.primary'
              }}>
                <NavigationDrawer />
                <Box component="main" sx={{ 
                  flexGrow: 1,
                  p: 3,
                  bgcolor: 'background.default',
                  color: 'text.primary'
                }}>
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/studio" element={<Studio />} />
                    <Route path="/filemanager" element={<FileManager />} />
                    <Route path="/setup" element={<Setup />} />
                    {/* In your routes, add this route (only accessible to admins): */}
                    <Route 
                      path="/users"
                      element={
                        <ProtectedRoute adminOnly>
                          <UserManagement />
                        </ProtectedRoute>
                      } 
                    />
                  </Routes>
                </Box>
              </Box>
            </ProtectedRoute>
          } />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;