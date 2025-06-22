import React, { useContext } from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Switch,
  FormControlLabel
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import MicIcon from '@mui/icons-material/Mic';
import AudioFileIcon from '@mui/icons-material/AudioFile';
import SettingsIcon from '@mui/icons-material/Settings';
import LoginIcon from '@mui/icons-material/Login';
import ThemeContext from '../../../contexts/ThemeContext';
import { useNavigate } from 'react-router-dom';

const drawerWidth = 240;

function NavigationDrawer() {
  const { theme, toggleTheme } = useContext(ThemeContext);
  const navigate = useNavigate();

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

  return (
    <Drawer
      variant="permanent"
      sx={drawerStyles}
    >
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
        <ListItem button key="Login" sx={listItemStyles} onClick={() => navigate('/login')}>
          <ListItemIcon><LoginIcon /></ListItemIcon>
          <ListItemText primary="Login" />
        </ListItem>
      </List>
    </Drawer>
  );
}

export default NavigationDrawer;