import React, { useContext, useState, useEffect } from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Switch,
  FormControlLabel,
  Typography,
  Box,
  Avatar,
  Divider,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import MicIcon from '@mui/icons-material/Mic';
import AudioFileIcon from '@mui/icons-material/AudioFile';
import SettingsIcon from '@mui/icons-material/Settings';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import RadioIcon from '@mui/icons-material/Radio';
import ThemeContext from '../../../contexts/ThemeContext';
import { useAuth } from '../../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const drawerWidth = 240;

function NavigationDrawer() {
  const { theme, toggleTheme } = useContext(ThemeContext);
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  
  // Station selection state
  const [stations, setStations] = useState([]);
  const [selectedStation, setSelectedStation] = useState('');
  const [loadingStations, setLoadingStations] = useState(false);

  // Load user preferences and stations when user is logged in
  useEffect(() => {
    if (user) {
      loadUserData();
    } else {
      // Clear data when user logs out
      setStations([]);
      setSelectedStation('');
      localStorage.removeItem('selectedStation');
    }
  }, [user]);

  // Listen for auth events
  useEffect(() => {
    const handleUserLoggedIn = () => {
      console.log('NavigationDrawer: User logged in, reloading data');
      if (user) {
        loadUserData();
      }
    };

    const handleUserLoggedOut = () => {
      console.log('NavigationDrawer: User logged out, clearing data');
      setStations([]);
      setSelectedStation('');
      localStorage.removeItem('selectedStation');
    };

    window.addEventListener('userLoggedIn', handleUserLoggedIn);
    window.addEventListener('userLoggedOut', handleUserLoggedOut);

    return () => {
      window.removeEventListener('userLoggedIn', handleUserLoggedIn);
      window.removeEventListener('userLoggedOut', handleUserLoggedOut);
    };
  }, [user]);

  const loadUserData = async () => {
    try {
      setLoadingStations(true);
      
      // Load stations and preferences
      const [stationsResponse, preferencesResponse] = await Promise.allSettled([
        axios.get('http://localhost:8000/api/stations'),
        axios.get('http://localhost:8000/api/preferences')
      ]);
      
      // Handle stations
      let stationsList = [];
      if (stationsResponse.status === 'fulfilled') {
        stationsList = stationsResponse.value.data;
        setStations(stationsList);
      } else {
        console.error('Error loading stations:', stationsResponse.reason);
        setStations([]);
      }
      
      // Handle preferences
      let savedStationId = null;
      if (preferencesResponse.status === 'fulfilled') {
        savedStationId = preferencesResponse.value.data.selectedStationId;
        console.log('Loaded saved station from server:', savedStationId);
      } else {
        console.error('Error loading preferences:', preferencesResponse.reason);
        // Fallback to localStorage
        const savedStationData = localStorage.getItem('selectedStation');
        if (savedStationData) {
          try {
            const savedStation = JSON.parse(savedStationData);
            savedStationId = savedStation.id;
          } catch (e) {
            console.error('Error parsing saved station data:', e);
          }
        }
      }
      
      // Set selected station
      if (savedStationId && stationsList.some(s => s.id === savedStationId)) {
        setSelectedStation(savedStationId);
        const station = stationsList.find(s => s.id === savedStationId);
        if (station) {
          localStorage.setItem('selectedStation', JSON.stringify(station));
        }
      } else if (stationsList.length > 0) {
        // Auto-select first station
        const firstStation = stationsList[0];
        setSelectedStation(firstStation.id);
        localStorage.setItem('selectedStation', JSON.stringify(firstStation));
        
        // Save to server
        try {
          await axios.put('http://localhost:8000/api/preferences', { 
            selectedStationId: firstStation.id 
          });
        } catch (error) {
          console.error('Error saving auto-selected station to server:', error);
        }
      }
      
    } catch (error) {
      console.error('Error loading user data:', error);
      setStations([]);
    } finally {
      setLoadingStations(false);
    }
  };

  const handleStationChange = async (event) => {
    const stationId = event.target.value;
    setSelectedStation(stationId);
    
    // Find the full station object and store it locally
    const station = stations.find(s => s.id === stationId);
    if (station) {
      localStorage.setItem('selectedStation', JSON.stringify(station));
      // Dispatch event to notify other components
      window.dispatchEvent(new CustomEvent('stationChanged', { detail: station }));
    }
    
    // Sync with server
    try {
      await axios.put('http://localhost:8000/api/preferences', { selectedStationId: stationId });
      console.log('Station selection synced with server:', stationId);
    } catch (error) {
      console.error('Error syncing station selection with server:', error);
      // Station selection still works locally even if server sync fails
    }
  };

  // Define theme-based styles
  const drawerStyles = {
    width: drawerWidth,
    flexShrink: 0,
    [`& .MuiDrawer-paper`]: { 
      width: drawerWidth, 
      boxSizing: 'border-box',
      backgroundColor: theme === 'dark' ? '#1e1e1e' : '#ffffff',
      color: theme === 'dark' ? '#ffffff' : '#000000',
      borderRight: theme === 'dark' ? '1px solid #333333' : '1px solid #e0e0e0'
    },
  };

  const listItemStyles = {
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: theme === 'dark' ? '#333333' : '#f5f5f5',
    },
    '& .MuiListItemIcon-root': {
      color: theme === 'dark' ? '#ffffff' : '#000000',
    },
    '& .MuiListItemText-primary': {
      color: theme === 'dark' ? '#ffffff' : '#000000',
    }
  };

  const switchStyles = {
    marginLeft: 1,
    '& .MuiFormControlLabel-label': {
      color: theme === 'dark' ? '#ffffff' : '#000000',
    },
    '& .MuiSwitch-switchBase.Mui-checked': {
      color: theme === 'dark' ? '#90caf9' : '#1976d2',
    },
    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
      backgroundColor: theme === 'dark' ? '#90caf9' : '#1976d2',
    }
  };

  const userSectionStyles = {
    padding: 2,
    backgroundColor: theme === 'dark' ? '#2a2a2a' : '#f5f5f5',
    borderBottom: theme === 'dark' ? '1px solid #333333' : '1px solid #e0e0e0',
  };

  const stationSectionStyles = {
    padding: 2,
    backgroundColor: theme === 'dark' ? '#252525' : '#fafafa',
    borderBottom: theme === 'dark' ? '1px solid #333333' : '1px solid #e0e0e0',
  };

  const selectStyles = {
    '& .MuiOutlinedInput-root': {
      color: theme === 'dark' ? '#ffffff' : '#000000',
      '& fieldset': {
        borderColor: theme === 'dark' ? '#333333' : '#e0e0e0',
      },
      '&:hover fieldset': {
        borderColor: theme === 'dark' ? '#555555' : '#cccccc',
      },
      '&.Mui-focused fieldset': {
        borderColor: theme === 'dark' ? '#90caf9' : '#1976d2',
      },
    },
    '& .MuiInputLabel-root': {
      color: theme === 'dark' ? '#b0b0b0' : '#666666',
    },
    '& .MuiSelect-icon': {
      color: theme === 'dark' ? '#ffffff' : '#000000',
    }
  };

  const handleAuthAction = () => {
    if (user) {
      // User is logged in, perform logout
      logout();
      navigate('/');
      // Clear selected station on logout
      setSelectedStation('');
      setStations([]);
      localStorage.removeItem('selectedStation');
    } else {
      // User is not logged in, navigate to login
      navigate('/login');
    }
  };

  return (
    <Drawer
      variant="permanent"
      sx={drawerStyles}
    >
      {/* User Section */}
      {user && (
        <>
          <Box sx={userSectionStyles}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar 
                sx={{ 
                  width: 32, 
                  height: 32,
                  bgcolor: theme === 'dark' ? '#90caf9' : '#1976d2',
                  fontSize: '0.875rem'
                }}
              >
                {user.username ? user.username.charAt(0).toUpperCase() : <PersonIcon />}
              </Avatar>
              <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                <Typography 
                  variant="subtitle2" 
                  sx={{ 
                    color: theme === 'dark' ? '#ffffff' : '#000000',
                    fontWeight: 600,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}
                >
                  {user.username || 'User'}
                </Typography>
                <Typography 
                  variant="caption" 
                  sx={{ 
                    color: theme === 'dark' ? '#b0b0b0' : '#666666',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}
                >
                  Logged in
                </Typography>
              </Box>
            </Box>
          </Box>
          <Divider />
        </>
      )}

      {/* Station Selection Section */}
      {user && stations.length > 0 && (
        <>
          <Box sx={stationSectionStyles}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0, mb: 0 }}>
            </Box>
            <FormControl fullWidth size="small" sx={selectStyles}>
              <InputLabel>Select Station</InputLabel>
              <Select
                value={selectedStation}
                onChange={handleStationChange}
                label="Select Station"
                disabled={loadingStations}
              >
                {loadingStations ? (
                  <MenuItem disabled>
                    <CircularProgress size={20} sx={{ mr: 1 }} />
                    Loading stations...
                  </MenuItem>
                ) : (
                  stations.map((station) => (
                    <MenuItem key={station.id} value={station.id}>
                      {station.callLetters}
                    </MenuItem>
                  ))
                )}
              </Select>
            </FormControl>
          </Box>
          <Divider />
        </>
      )}

      <List>
        <ListItem button key="Home" sx={listItemStyles} onClick={() => navigate('/')}>
          <ListItemIcon><HomeIcon /></ListItemIcon>
          <ListItemText primary="Home" />
        </ListItem>
        <ListItem button key="Studio" sx={listItemStyles} onClick={() => navigate('/studio')}>
          <ListItemIcon><MicIcon /></ListItemIcon>
          <ListItemText primary="Studio" />
        </ListItem>
        <ListItem button key="FileManager" sx={listItemStyles} onClick={() => navigate('/filemanager')}>
          <ListItemIcon><AudioFileIcon /></ListItemIcon>
          <ListItemText primary="File Manager" />
        </ListItem>
        <ListItem button key="Setup" sx={listItemStyles} onClick={() => navigate('/setup')}>
          <ListItemIcon><SettingsIcon /></ListItemIcon>
          <ListItemText primary="Setup" />
        </ListItem>
      </List>
      <List sx={{ marginTop: 'auto' }}>
        <ListItem key="ThemeToggle">
        <FormControlLabel
          control={<Switch checked={theme === 'dark'} onChange={toggleTheme} />}
          label={`${theme === 'light' ? 'Light' : 'Dark'} Mode`}
          sx={switchStyles}
        />
        </ListItem>
        <ListItem button key="AuthAction" sx={listItemStyles} onClick={handleAuthAction}>
          <ListItemIcon>
            {user ? <LogoutIcon /> : <LoginIcon />}
          </ListItemIcon>
          <ListItemText primary={user ? "Logout" : "Login"} />
        </ListItem>
      </List>
    </Drawer>
  );
}

export default NavigationDrawer;