import React, { useContext } from 'react';
import { Box, Typography, Paper } from '@mui/material';
import ThemeContext from '../../contexts/ThemeContext';

function Home() {
  const { theme } = useContext(ThemeContext);

  const containerStyles = {
    padding: 3,
    backgroundColor: theme === 'dark' ? '#121212' : '#ffffff',
    color: theme === 'dark' ? '#ffffff' : '#000000',
    minHeight: '100vh'
  };

  const paperStyles = {
    padding: 3,
    backgroundColor: theme === 'dark' ? '#1e1e1e' : '#f5f5f5',
    color: theme === 'dark' ? '#ffffff' : '#000000',
    marginBottom: 2
  };

  const headingStyles = {
    color: theme === 'dark' ? '#90caf9' : '#1976d2',
    marginBottom: 2
  };

  return (
    <Box sx={containerStyles}>
      <Typography variant="h3" component="h1" sx={headingStyles}>
        Home Page
      </Typography>
      <Paper sx={paperStyles} elevation={theme === 'dark' ? 0 : 1}>
        <Typography variant="h6" component="p">
          Welcome to the home page!
        </Typography>
        <Typography variant="body1" sx={{ marginTop: 2 }}>
          This application supports both light and dark themes. You can toggle between themes using the switch in the navigation drawer.
        </Typography>
      </Paper>
    </Box>
  );
}

export default Home;