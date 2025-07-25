import React, { useState } from 'react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import Login from './components/auth/Login.jsx';
import Register from './components/auth/Register';
import Dashboard from './components/dashboard/Dashboard.jsx';
import Profile from './components/profile/Profile';
import Settings from './components/settings/Settings';
import NavigationDrawer from './components/ui/NavigationDrawer.tsx';
import { useAuth } from './contexts/AuthContext';

const theme = createTheme();

// Main App Content Component
const AppContent = () => {
  const { user } = useAuth();
  const [currentView, setCurrentView] = useState('dashboard');
  const [authView, setAuthView] = useState('login'); // 'login' or 'register'

  // If user is not logged in, show auth views
  if (!user) {
    return (
      <>
        {authView === 'login' ? (
          <Login onSwitchToRegister={() => setAuthView('register')} />
        ) : (
          <Register onSwitchToLogin={() => setAuthView('login')} />
        )}
      </>
    );
  }

  // If user is logged in, show main app with navigation
  const renderCurrentView = () => {
    switch (currentView) {
      case 'dashboard':
        return <Dashboard />;
      case 'profile':
        return <Profile />;
      case 'settings':
        return <Settings />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <NavigationDrawer currentView={currentView} onViewChange={setCurrentView}>
      {renderCurrentView()}
    </NavigationDrawer>
  );
};

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;