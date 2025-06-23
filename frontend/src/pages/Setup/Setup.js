import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  Button,
  Typography,
  List,
  ListItem,
  ListItemText,
  Paper,
  Divider,
  Alert,
  CircularProgress
} from '@mui/material';
import { Add as AddIcon, Radio as RadioIcon } from '@mui/icons-material';
import axios from 'axios';

function Setup() {
  const [callLetters, setCallLetters] = useState('');
  const [stations, setStations] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Fetch stations on component mount
  useEffect(() => {
    fetchStations();
  }, []);

  const fetchStations = async () => {
    try {
      setLoading(true);
      setError(''); // Clear any previous errors
      
      console.log('Fetching stations...');
      console.log('Authorization header:', axios.defaults.headers.common['Authorization']);
      
      const response = await axios.get('/api/stations');
      console.log('Stations response:', response.data);
      setStations(response.data);
    } catch (err) {
      console.error('Detailed error fetching stations:', err);
      console.error('Error response:', err.response);
      console.error('Error status:', err.response?.status);
      console.error('Error data:', err.response?.data);
      
      let errorMessage = 'Failed to fetch stations';
      
      if (err.response?.status === 401) {
        errorMessage = 'Authentication required. Please log in again.';
      } else if (err.response?.status === 403) {
        errorMessage = 'Access denied';
      } else if (err.response?.status === 404) {
        errorMessage = 'API endpoint not found';
      } else if (err.response?.data?.detail) {
        errorMessage = err.response.data.detail;
      } else if (err.code === 'ECONNREFUSED') {
        errorMessage = 'Cannot connect to server. Is the backend running?';
      } else if (err.message) {
        errorMessage = `Network error: ${err.message}`;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!callLetters.trim()) {
      setError('Call letters are required');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');

      console.log('Adding station:', callLetters.trim().toUpperCase());
      
      const response = await axios.post('/api/stations', {
        callLetters: callLetters.trim().toUpperCase()
      });

      console.log('Station added:', response.data);
      setSuccess('Station added successfully!');
      setCallLetters('');
      
      // Refresh the stations list
      await fetchStations();
      
    } catch (err) {
      console.error('Error adding station:', err);
      console.error('Error response:', err.response);
      
      let errorMessage = 'Failed to add station';
      
      if (err.response?.status === 401) {
        errorMessage = 'Authentication required. Please log in again.';
      } else if (err.response?.data?.detail) {
        errorMessage = err.response.data.detail;
      } else if (err.response?.data?.message) {
        errorMessage = err.response.data.message;
      } else if (err.message) {
        errorMessage = `Network error: ${err.message}`;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleCallLettersChange = (e) => {
    // Convert to uppercase and limit to typical call letter format
    const value = e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (value.length <= 8) { // Typical max length for call letters
      setCallLetters(value);
    }
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Station Setup
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Add and manage radio stations for your studio.
      </Typography>

      {/* Add Station Form */}
      <Paper elevation={2} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Add New Station
        </Typography>
        
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}
        
        {success && (
          <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess('')}>
            {success}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
          <TextField
            label="Call Letters"
            value={callLetters}
            onChange={handleCallLettersChange}
            placeholder="e.g., WXYZ"
            variant="outlined"
            size="medium"
            sx={{ flexGrow: 1 }}
            disabled={loading}
            helperText="Enter station call letters (letters and numbers only)"
          />
          <Button
            type="submit"
            variant="contained"
            startIcon={loading ? <CircularProgress size={20} /> : <AddIcon />}
            disabled={loading || !callLetters.trim()}
            sx={{ mt: 0.5 }}
          >
            Add Station
          </Button>
        </Box>
      </Paper>

      {/* Stations List */}
      <Paper elevation={2} sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Current Stations
          <Button 
            onClick={fetchStations} 
            size="small" 
            sx={{ ml: 2 }}
            disabled={loading}
          >
            Refresh
          </Button>
        </Typography>
        
        {loading && stations.length === 0 ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : stations.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No stations added yet. Add your first station above.
          </Typography>
        ) : (
          <List>
            {stations.map((station, index) => (
              <React.Fragment key={station.id || index}>
                <ListItem>
                  <RadioIcon sx={{ mr: 2, color: 'primary.main' }} />
                  <ListItemText
                    primary={station.callLetters}
                    secondary={`Added: ${new Date(station.createdAt || Date.now()).toLocaleDateString()}`}
                  />
                </ListItem>
                {index < stations.length - 1 && <Divider />}
              </React.Fragment>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
}

export default Setup;