// src/components/ResetPassword.jsx
import { useState, useEffect } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import api from '../services/api';

function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (!token) {
      setError('Missing reset token. Please request a new password reset link.');
    }
  }, [token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validate passwords
    if (newPassword !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }
    
    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters long');
      return;
    }
    
    setError('');
    setIsSubmitting(true);
    
    try {
      await api.post('/api/auth/reset-password', {
        token,
        new_password: newPassword
      });
      
      setSuccess(true);
      
      // Redirect to login after 3 seconds
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (error) {
      console.error('Password reset error:', error);
      let message = 'Failed to reset password';
      
      if (error.response && error.response.data && error.response.data.detail) {
        message = error.response.data.detail;
      }
      
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (success) {
    return (
      <div className="reset-password-container">
        <h2>Password Reset Successful</h2>
        <p>Your password has been reset successfully.</p>
        <p>You will be redirected to the login page shortly.</p>
        <p>
          <Link to="/login">Click here if you are not redirected</Link>
        </p>
      </div>
    );
  }

  return (
    <div className="reset-password-container">
      <h2>Reset Password</h2>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="newPassword">New Password</label>
          <input
            type="password"
            id="newPassword"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            disabled={isSubmitting || !token}
            minLength={8}
          />
        </div>
        <div className="form-group">
          <label htmlFor="confirmPassword">Confirm Password</label>
          <input
            type="password"
            id="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            disabled={isSubmitting || !token}
            minLength={8}
          />
        </div>
        <button type="submit" disabled={isSubmitting || !token}>
          {isSubmitting ? 'Resetting...' : 'Reset Password'}
        </button>
      </form>
      <p>
        <Link to="/login">Return to login</Link>
      </p>
    </div>
  );
}

export default ResetPassword;