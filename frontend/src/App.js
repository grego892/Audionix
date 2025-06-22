import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Box } from '@mui/material';
import Home from './pages/Home/Home.js';
import Studio from './pages/Studio/Studio.js';
import FileManager from "./pages/FileManager/FileManager";
import Setup from "./pages/Setup/Setup";
import NavigationDrawer from './components/layout/NavigationDrawer/NavigationDrawer.js';
import ThemeContext from './contexts/ThemeContext';

function App() {
  const { theme } = useContext(ThemeContext);

  const appStyles = {
    display: 'flex',
    backgroundColor: theme === 'dark' ? '#121212' : '#ffffff',
    minHeight: '100vh',
    color: theme === 'dark' ? '#ffffff' : '#000000'
  };

  const mainStyles = {
    flexGrow: 1,
    p: 3,
    backgroundColor: theme === 'dark' ? '#121212' : '#ffffff',
    color: theme === 'dark' ? '#ffffff' : '#000000'
  };

  return (
    <Router>
      <Box sx={appStyles}>
        <NavigationDrawer />
        <Box component="main" sx={mainStyles}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/studio" element={<Studio />} />
            <Route path="/filemanager" element={<FileManager />} />
            <Route path="/setup" element={<Setup />} />
          </Routes>
        </Box>
      </Box>
    </Router>
  );
}

export default App;