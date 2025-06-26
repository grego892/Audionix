import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Grid,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  Chip,
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
  Tooltip
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Delete as DeleteIcon,
  Download as DownloadIcon,
  AudioFile as AudioFileIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import axios from 'axios';
import { useAuth } from '../../contexts/AuthContext';

function FileManager() {
  const { user } = useAuth();
  const [audioFiles, setAudioFiles] = useState([]);
  const [stations, setStations] = useState([]);
  const [selectedStation, setSelectedStation] = useState('');
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [uploadDialog, setUploadDialog] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const [fileToDelete, setFileToDelete] = useState(null);
  const [selectedFiles, setSelectedFiles] = useState([]);

  useEffect(() => {
    if (user) {
      loadStations();
      loadAudioFiles();
    }
  }, [user]);

  useEffect(() => {
    if (selectedStation) {
      loadAudioFiles();
    }
  }, [selectedStation]);

  const loadStations = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('http://localhost:8000/api/stations', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setStations(response.data);
    } catch (error) {
      console.error('Error loading stations:', error);
      setError('Failed to load stations');
    }
  };

  const loadAudioFiles = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const params = selectedStation ? { station_id: selectedStation } : {};
      const response = await axios.get('http://localhost:8000/api/audio-files', {
        headers: { Authorization: `Bearer ${token}` },
        params
      });
      setAudioFiles(response.data);
    } catch (error) {
      console.error('Error loading audio files:', error);
      setError('Failed to load audio files');
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (files) => {
    if (!files || files.length === 0) return;

    setUploading(true);
    setUploadProgress(0);
    const token = localStorage.getItem('token');

    try {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const formData = new FormData();
        formData.append('file', file);
        if (selectedStation) {
          formData.append('station_id', selectedStation);
        }

        await axios.post('http://localhost:8000/api/upload-audio', formData, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          },
          onUploadProgress: (progressEvent) => {
            const progress = Math.round(
              ((i + (progressEvent.loaded / progressEvent.total)) / files.length) * 100
            );
            setUploadProgress(progress);
          }
        });
      }

      setSuccess(`Successfully uploaded ${files.length} file(s)`);
      setUploadDialog(false);
      loadAudioFiles();
    } catch (error) {
      console.error('Error uploading files:', error);
      setError(error.response?.data?.detail || 'Failed to upload files');
    } finally {
      setUploading(false);
      setUploadProgress(0);
    }
  };

  const handleDeleteFile = async (fileId) => {
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`http://localhost:8000/api/audio-files/${fileId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setSuccess('File deleted successfully');
      setDeleteDialog(false);
      setFileToDelete(null);
      loadAudioFiles();
    } catch (error) {
      console.error('Error deleting file:', error);
      setError('Failed to delete file');
    }
  };

  const handleDownloadFile = async (fileId, originalName) => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`http://localhost:8000/api/audio-files/${fileId}/download`, {
        headers: { Authorization: `Bearer ${token}` },
        responseType: 'blob'
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', originalName);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading file:', error);
      setError('Failed to download file');
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  const clearMessages = () => {
    setError('');
    setSuccess('');
  };

  if (!user) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">
          Please log in to access the File Manager.
        </Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          File Manager
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<UploadIcon />}
            onClick={() => setUploadDialog(true)}
            disabled={uploading}
          >
            Upload Files
          </Button>
          <IconButton onClick={loadAudioFiles} disabled={loading}>
            <RefreshIcon />
          </IconButton>
        </Box>
      </Box>

      {/* Station Filter */}
      <Box sx={{ mb: 3 }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Filter by Station</InputLabel>
          <Select
            value={selectedStation}
            onChange={(e) => setSelectedStation(e.target.value)}
            label="Filter by Station"
          >
            <MenuItem value="">All Stations</MenuItem>
            {stations.map((station) => (
              <MenuItem key={station.id} value={station.id}>
                {station.callLetters}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {/* Messages */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearMessages}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={clearMessages}>
          {success}
        </Alert>
      )}

      {/* File List */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Audio Files ({audioFiles.length})
          </Typography>
          
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <LinearProgress sx={{ width: '100%' }} />
            </Box>
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Size</TableCell>
                    <TableCell>Station</TableCell>
                    <TableCell>Uploaded By</TableCell>
                    <TableCell>Upload Date</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {audioFiles.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} align="center">
                        <Typography variant="body2" color="text.secondary">
                          No audio files found
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    audioFiles.map((file) => (
                      <TableRow key={file.id}>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <AudioFileIcon color="primary" />
                            <Tooltip title={file.originalName}>
                              <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                                {file.originalName}
                              </Typography>
                            </Tooltip>
                          </Box>
                        </TableCell>
                        <TableCell>{formatFileSize(file.fileSize)}</TableCell>
                        <TableCell>
                          {file.stationId ? (
                            <Chip
                              label={stations.find(s => s.id === file.stationId)?.callLetters || 'Unknown'}
                              size="small"
                              variant="outlined"
                            />
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              No Station
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell>{file.uploadedBy}</TableCell>
                        <TableCell>{formatDate(file.uploadedAt)}</TableCell>
                        <TableCell align="right">
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <IconButton
                              size="small"
                              onClick={() => handleDownloadFile(file.id, file.originalName)}
                              title="Download"
                            >
                              <DownloadIcon />
                            </IconButton>
                            <IconButton
                              size="small"
                              onClick={() => {
                                setFileToDelete(file);
                                setDeleteDialog(true);
                              }}
                              title="Delete"
                              color="error"
                            >
                              <DeleteIcon />
                            </IconButton>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Upload Dialog */}
      <Dialog open={uploadDialog} onClose={() => !uploading && setUploadDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Audio Files</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <input
              type="file"
              multiple
              accept="audio/*"
              onChange={(e) => setSelectedFiles(Array.from(e.target.files))}
              style={{ display: 'none' }}
              id="file-upload"
            />
            <label htmlFor="file-upload">
              <Button
                variant="outlined"
                component="span"
                startIcon={<UploadIcon />}
                fullWidth
                sx={{ mb: 2 }}
              >
                Choose Audio Files
              </Button>
            </label>
            
            {selectedFiles.length > 0 && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Selected Files ({selectedFiles.length}):
                </Typography>
                {selectedFiles.map((file, index) => (
                  <Typography key={index} variant="body2" color="text.secondary">
                    {file.name} ({formatFileSize(file.size)})
                  </Typography>
                ))}
              </Box>
            )}

            {uploading && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" gutterBottom>
                  Uploading... {uploadProgress}%
                </Typography>
                <LinearProgress variant="determinate" value={uploadProgress} />
              </Box>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUploadDialog(false)} disabled={uploading}>
            Cancel
          </Button>
          <Button
            onClick={() => handleFileUpload(selectedFiles)}
            variant="contained"
            disabled={selectedFiles.length === 0 || uploading}
          >
            Upload
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialog} onClose={() => setDeleteDialog(false)}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete "{fileToDelete?.originalName}"?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog(false)}>Cancel</Button>
          <Button
            onClick={() => handleDeleteFile(fileToDelete?.id)}
            color="error"
            variant="contained"
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default FileManager;