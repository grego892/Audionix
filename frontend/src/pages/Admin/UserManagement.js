// pages/Admin/UserManagement.js
import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Checkbox,
  Alert,
  IconButton,
  Container,
  Menu,
  MenuItem
} from '@mui/material';
import { 
  Delete as DeleteIcon, 
  Add as AddIcon, 
  MoreVert as MoreVertIcon,
  Lock as LockIcon 
} from '@mui/icons-material';
import axios from 'axios';

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [openPasswordDialog, setOpenPasswordDialog] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const [passwordDialogUser, setPasswordDialogUser] = useState(null); // Separate state for password dialog
  const [anchorEl, setAnchorEl] = useState(null);
  const [newUser, setNewUser] = useState({
    username: '',
    password: '',
    isAdmin: false
  });
  const [passwordData, setPasswordData] = useState({
    newPassword: '',
    confirmPassword: ''
  });
  const [formError, setFormError] = useState('');

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('/api/admin/users', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setUsers(response.data);
      setError('');
    } catch (err) {
      setError('Failed to fetch users');
      console.error('Error fetching users:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateUser = async () => {
    setFormError('');
    
    if (!newUser.username || !newUser.password) {
      setFormError('Username and password are required');
      return;
    }

    if (newUser.password.length < 6) {
      setFormError('Password must be at least 6 characters long');
      return;
    }

    try {
      const token = localStorage.getItem('token');
      await axios.post('/api/admin/users', newUser, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      setSuccess('User created successfully');
      setOpenDialog(false);
      setNewUser({ username: '', password: '', isAdmin: false });
      fetchUsers();
    } catch (err) {
      setFormError(err.response?.data?.detail || 'Failed to create user');
    }
  };

  const handleDeleteUser = async (userId) => {
    if (!window.confirm('Are you sure you want to delete this user?')) {
      return;
    }

    try {
      const token = localStorage.getItem('token');
      await axios.delete(`/api/admin/users/${userId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      setSuccess('User deleted successfully');
      fetchUsers();
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to delete user');
    }
  };

  const handleChangePassword = async () => {
    setFormError('');
    
    if (!passwordData.newPassword || !passwordData.confirmPassword) {
      setFormError('Both password fields are required');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      setFormError('Password must be at least 6 characters long');
      return;
    }

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setFormError('Passwords do not match');
      return;
    }

    // Use passwordDialogUser instead of selectedUser
    if (!passwordDialogUser) {
      setFormError('No user selected');
      return;
    }

    try {
      const token = localStorage.getItem('token');
      
      // Add debug logging
      console.log('Changing password for user:', passwordDialogUser.id);
      console.log('API URL:', `/api/admin/users/${passwordDialogUser.id}/password`);
      
      const response = await axios.put(`/api/admin/users/${passwordDialogUser.id}/password`, {
        newPassword: passwordData.newPassword
      }, {
        headers: { 
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      console.log('Password change response:', response.data);
      
      setSuccess(`Password changed successfully for ${passwordDialogUser.username}`);
      setOpenPasswordDialog(false);
      setPasswordData({ newPassword: '', confirmPassword: '' });
      setPasswordDialogUser(null);
    } catch (err) {
      console.error('Password change error:', err);
      console.error('Error response:', err.response?.data);
      
      // More detailed error handling
      let errorMessage = 'Failed to change password';
      if (err.response?.data?.detail) {
        errorMessage = err.response.data.detail;
      } else if (err.response?.status === 401) {
        errorMessage = 'Unauthorized - please log in again';
      } else if (err.response?.status === 403) {
        errorMessage = 'Access denied - admin privileges required';
      } else if (err.response?.status === 404) {
        errorMessage = 'User not found';
      } else if (err.message) {
        errorMessage = err.message;
      }
      
      setFormError(errorMessage);
    }
  };

  const handleMenuClick = (event, user) => {
    setAnchorEl(event.currentTarget);
    setSelectedUser(user);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    // Don't set selectedUser to null here immediately
    // Wait for the dialog actions to complete
    setTimeout(() => {
      if (!openPasswordDialog) {
        setSelectedUser(null);
      }
    }, 100);
  };

  const handlePasswordDialogOpen = () => {
    // Store the selected user in a separate state for the password dialog
    setPasswordDialogUser(selectedUser);
    setOpenPasswordDialog(true);
    handleMenuClose();
    setFormError('');
    setPasswordData({ newPassword: '', confirmPassword: '' });
  };

  const handlePasswordDialogClose = () => {
    setOpenPasswordDialog(false);
    setPasswordData({ newPassword: '', confirmPassword: '' });
    setFormError('');
    setPasswordDialogUser(null);
    setSelectedUser(null);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <Container>
        <Typography>Loading users...</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">
            User Management
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setOpenDialog(true)}
          >
            Add User
          </Button>
        </Box>

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

        <Paper elevation={2}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Username</TableCell>
                  <TableCell>Role</TableCell>
                  <TableCell>Created</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>{user.username}</TableCell>
                    <TableCell>
                      <Typography
                        variant="body2"
                        color={user.isAdmin ? 'primary' : 'textSecondary'}
                        sx={{ fontWeight: user.isAdmin ? 'bold' : 'normal' }}
                      >
                        {user.isAdmin ? 'Admin' : 'User'}
                      </Typography>
                    </TableCell>
                    <TableCell>{formatDate(user.createdAt)}</TableCell>
                    <TableCell align="right">
                      <IconButton
                        onClick={(event) => handleMenuClick(event, user)}
                      >
                        <MoreVertIcon />
                      </IconButton>
                      <IconButton
                        color="error"
                        onClick={() => handleDeleteUser(user.id)}
                        disabled={user.isAdmin && users.filter(u => u.isAdmin).length === 1}
                        title={user.isAdmin && users.filter(u => u.isAdmin).length === 1 ? 'Cannot delete the last admin' : 'Delete user'}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>

        {/* Actions Menu */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
        >
          <MenuItem onClick={handlePasswordDialogOpen}>
            <LockIcon sx={{ mr: 1 }} />
            Change Password
          </MenuItem>
        </Menu>

        {/* Create User Dialog */}
        <Dialog
          open={openDialog}
          onClose={() => {
            setOpenDialog(false);
            setNewUser({ username: '', password: '', isAdmin: false });
            setFormError('');
          }}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>Add New User</DialogTitle>
          <DialogContent>
            {formError && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {formError}
              </Alert>
            )}
            
            <TextField
              autoFocus
              margin="dense"
              label="Username"
              fullWidth
              variant="outlined"
              value={newUser.username}
              onChange={(e) => setNewUser({ ...newUser, username: e.target.value })}
              sx={{ mb: 2 }}
            />
            
            <TextField
              margin="dense"
              label="Password"
              type="password"
              fullWidth
              variant="outlined"
              value={newUser.password}
              onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
              helperText="Password must be at least 6 characters long"
              sx={{ mb: 2 }}
            />
            
            <FormControlLabel
              control={
                <Checkbox
                  checked={newUser.isAdmin}
                  onChange={(e) => setNewUser({ ...newUser, isAdmin: e.target.checked })}
                />
              }
              label="Administrator privileges"
            />
          </DialogContent>
          <DialogActions>
            <Button
              onClick={() => {
                setOpenDialog(false);
                setNewUser({ username: '', password: '', isAdmin: false });
                setFormError('');
              }}
            >
              Cancel
            </Button>
            <Button onClick={handleCreateUser} variant="contained">
              Create User
            </Button>
          </DialogActions>
        </Dialog>

        {/* Change Password Dialog */}
        <Dialog
          open={openPasswordDialog}
          onClose={handlePasswordDialogClose}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>
            Change Password for {passwordDialogUser?.username}
          </DialogTitle>
          <DialogContent>
            {formError && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {formError}
              </Alert>
            )}
            
            <TextField
              autoFocus
              margin="dense"
              label="New Password"
              type="password"
              fullWidth
              variant="outlined"
              value={passwordData.newPassword}
              onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
              helperText="Password must be at least 6 characters long"
              sx={{ mb: 2 }}
            />
            
            <TextField
              margin="dense"
              label="Confirm New Password"
              type="password"
              fullWidth
              variant="outlined"
              value={passwordData.confirmPassword}
              onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
              sx={{ mb: 2 }}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={handlePasswordDialogClose}>
              Cancel
            </Button>
            <Button onClick={handleChangePassword} variant="contained">
              Change Password
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Container>
  );
};

export default UserManagement;