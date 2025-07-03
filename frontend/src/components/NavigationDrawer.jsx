// src/components/NavigationDrawer.jsx
import { useState, useContext } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from '../contexts/AuthContext';
import './NavigationDrawer.css';

function NavigationDrawer() {
  const [isOpen, setIsOpen] = useState(false);
  const { user, logout } = useContext(AuthContext);

  const toggleDrawer = () => {
    setIsOpen(!isOpen);
  };

  return (
    <>
      <button className="drawer-toggle" onClick={toggleDrawer}>
        ☰
      </button>
      <div className={`navigation-drawer ${isOpen ? 'open' : ''}`}>
        <div className="drawer-header">
          <button className="close-drawer" onClick={toggleDrawer}>
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
              <Link to="/" onClick={toggleDrawer}>
                Home
              </Link>
            </li>
            {user ? (
              <>
                <li>
                  <Link to="/dashboard" onClick={toggleDrawer}>
                    Dashboard
                  </Link>
                </li>
                <li>
                  <Link to="/profile" onClick={toggleDrawer}>
                    Profile
                  </Link>
                </li>
                <li>
                  <button className="logout-button" onClick={logout}>
                    Logout
                  </button>
                </li>
              </>
            ) : (
              <>
                <li>
                  <Link to="/login" onClick={toggleDrawer}>
                    Login
                  </Link>
                </li>
                <li>
                  <Link to="/register" onClick={toggleDrawer}>
                    Register
                  </Link>
                </li>
              </>
            )}
          </ul>
        </nav>
      </div>
      {isOpen && <div className="drawer-backdrop" onClick={toggleDrawer}></div>}
    </>
  );
}

export default NavigationDrawer;