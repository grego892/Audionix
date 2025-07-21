import React from 'react';
import { Typography, Container, Paper, Box } from '@mui/material';

const Dashboard = () => {
  return (
    <Container maxWidth="lg">
      <Paper elevation={3} sx={{ p: 3, mt: 2 }}>
        <Typography variant="h4" gutterBottom>
          Dashboard
        </Typography>
        <Typography variant="body1">
          Welcome to your dashboard! This is where your main content will go.
        </Typography>
        {/* Add your dashboard content here */}
      </Paper>
    </Container>
  );
};

export default Dashboard;