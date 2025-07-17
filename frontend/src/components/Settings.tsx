import React from 'react';
import { Container, Paper, Typography } from '@mui/material';

const Settings = () => {
  return (
    <Container maxWidth="lg">
      <Paper elevation={3} sx={{ p: 3, mt: 2 }}>
        <Typography variant="h4" gutterBottom>
          Settings
        </Typography>
        <Typography variant="body1">
          Application settings and preferences will go here.
        </Typography>
      </Paper>
    </Container>
  );
};

export default Settings;