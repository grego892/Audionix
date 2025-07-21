import React, { useState } from 'react';
import { 
  Container, 
  Paper, 
  TextField, 
  Button, 
  Typography, 
  Box, 
  Alert 
} from '@mui/material';
import { useAuth } from '../../contexts/AuthContext.jsx';

const Register = ({ onSwitchToLogin }) => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    full_name: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    setLoading(true);
    
    const { confirmPassword, ...userData } = formData;
    const result = await register(userData);
    
    if (result.success) {
      // Switch to login after successful registration
      onSwitchToLogin();
    } else {
      setError(result.error);
    }
    
    setLoading(false);
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 8 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom align="center">
          Create Account
        </Typography>
        
        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          
          <TextField
            name="username"
            label="Username"
            variant="outlined"
            fullWidth
            margin="normal"
            required
            value={formData.username}
            onChange={handleChange}
          />
          
          <TextField
            name="email"
            label="Email"
            type="email"
            variant="outlined"
            fullWidth
            margin="normal"
            required
            value={formData.email}
            onChange={handleChange}
          />
          
          <TextField
            name="full_name"
            label="Full Name"
            variant="outlined"
            fullWidth
            margin="normal"
            required
            value={formData.full_name}
            onChange={handleChange}
          />
          
          <TextField
            name="password"
            label="Password"
            type="password"
            variant="outlined"
            fullWidth
            margin="normal"
            required
            value={formData.password}
            onChange={handleChange}
          />
          
          <TextField
            name="confirmPassword"
            label="Confirm Password"
            type="password"
            variant="outlined"
            fullWidth
            margin="normal"
            required
            value={formData.confirmPassword}
            onChange={handleChange}
          />
          
          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={loading}
            sx={{ mt: 3, mb: 2 }}
          >
            {loading ? 'Creating account...' : 'Sign Up'}
          </Button>
          
          <Button
            variant="text"
            fullWidth
            onClick={onSwitchToLogin}
          >
            Already have an account? Sign in
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Register;