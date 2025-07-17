import React from 'react';
import { Container, Paper, Typography } from '@mui/material';

const Profile = () => {
  return (
    <Container maxWidth="lg">
      <Paper elevation={3} sx={{ p: 3, mt: 2 }}>
        <Typography variant="h4" gutterBottom>
          Profile
        </Typography>
        <Typography variant="body1">
          User profile information and settings will go here.
        </Typography>
      </Paper>
    </Container>
  );
};

export default Profile;
