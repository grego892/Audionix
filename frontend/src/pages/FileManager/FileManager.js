import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Tooltip,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  Download as DownloadIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import axios from 'axios';
import { useAuth } from '../../contexts/AuthContext';
import BASE_URL from '../../config/apiConfig'; // Import base URL

function FileManager() {
  const { user } = useAuth();
  const [audioFiles, setAudioFiles] = useState([]);
  const [stations, setStations] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    if (user) {
      loadStations();
      loadAudioFiles();
    }
  }, [user]);

  const loadStations = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${BASE_URL}/api/stations`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setStations(response.data);
    } catch (error) {
      console.error('Error loading stations:', error);
      setError('Failed to load stations');
    }
  };

  const loadAudioFiles = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${BASE_URL}/api/audio-files`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setAudioFiles(response.data);
    } catch (error) {
      console.error('Error loading audio files:', error);
      setError('Failed to load audio files');
    }
  };

  return (
    <Box>
      <Typography variant="h4" component="h1">
        File Manager
      </Typography>
    </Box>
  );
}

export default FileManager;