// src/components/NavigationDrawer.jsx
import { useState, useContext } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from '../contexts/AuthContext';
import './NavigationDrawer.css';

function NavigationDrawer() {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const { user, logout } = useContext(AuthContext);

  const toggleDrawer = () => {
    setIsOpen(!isOpen);
  };

  const closeDrawer = () => {
    setIsOpen(false);
  };

  const handleLogout = async () => {
    setIsLoggingOut(true);
    await logout();
    setIsLoggingOut(false);
    closeDrawer();
  };

  return (
    <>
      <button 
        className="drawer-toggle" 
        onClick={toggleDrawer}
        aria-label="Open navigation menu"
      >
        ☰
      </button>
      <div className={`navigation-drawer ${isOpen ? 'open' : ''}`}>
        <div className="drawer-header">
          <button 
            className="close-drawer" 
            onClick={closeDrawer}
            aria-label="Close navigation menu"
          >
            ×
          </button>
          {user && (
            <div className="user-info">
              <h3>{user.name}</h3>
              <p>{user.email}</p>
            </div>
          )}
        </div>
        <nav className="drawer-nav">
          <ul>
            <li>
              <Link to="/" onClick={closeDrawer}>
                Home
              </Link>
            </li>
            {user ? (
              <>
                <li>
                  <Link to="/dashboard" onClick={closeDrawer}>
                    Dashboard
                  </Link>
                </li>
                <li>
                  <Link to="/profile" onClick={closeDrawer}>
                    Profile
                  </Link>
                </li>
                <li>
                  <button 
                    className="logout-button" 
                    onClick={handleLogout}
                    disabled={isLoggingOut}
                  >
                    {isLoggingOut ? 'Logging out...' : 'Logout'}
                  </button>
                </li>
              </>
            ) : (
              <>
                <li>
                  <Link to="/login" onClick={closeDrawer}>
                    Login
                  </Link>
                </li>
                <li>
                  <Link to="/register" onClick={closeDrawer}>
                    Register
                  </Link>
                </li>
              </>
            )}
          </ul>
        </nav>
      </div>
      {isOpen && <div className="drawer-backdrop" onClick={closeDrawer}></div>}
    </>
  );
}

export default NavigationDrawer;