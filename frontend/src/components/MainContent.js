import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@mui/material';
import axios from 'axios';

function MainContent() {
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    axios.get('http://localhost:8000/api/hello')
      .then(res => setMessage(res.data.message))
      .catch(err => {
        console.error(err);
        setError('Failed to fetch message from the API');
      });
  }, []);

  return (
    <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
      {error ? (
        <Typography variant="h6" color="error">{error}</Typography>
      ) : (
        <Typography variant="h4">{message}</Typography>
      )}
    </Box>
  );
}

export default MainContent;