import React from 'react';
import { Box, Typography, Paper } from '@mui/material';

function Home() {
  return (
    <Box sx={{ p: 3 }}>
      <Typography 
        variant="h3" 
        component="h1" 
        color="primary"
        sx={{ mb: 2 }}
      >
        Home Page
      </Typography>
      <Paper sx={{ p: 3, mb: 2 }} elevation={1}>
        <Typography variant="h6" component="p">
          Welcome to the home page!
        </Typography>
        <Typography variant="body1" sx={{ mt: 2 }}>
          This application supports both light and dark themes. You can toggle between themes using the switch in the navigation drawer.
        </Typography>
      </Paper>
    </Box>
  );
}

export default Home;