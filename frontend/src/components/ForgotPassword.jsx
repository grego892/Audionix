// src/components/ForgotPassword.jsx
import { useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../services/api';

function ForgotPassword() {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    
    try {
      await api.post('/api/auth/forgot-password', { email });
      setSubmitted(true);
    } catch (error) {
      console.error('Password reset request error:', error);
      let message = 'Failed to process your request';
      
      if (error.response && error.response.data && error.response.data.detail) {
        message = error.response.data.detail;
      }
      
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (submitted) {
    return (
      <div className="forgot-password-container">
        <h2>Password Reset Email Sent</h2>
        <p>
          If an account exists with the email you provided, you will receive 
          password reset instructions shortly.
        </p>
        <p>
          <Link to="/login">Return to login</Link>
        </p>
      </div>
    );
  }

  return (
    <div className="forgot-password-container">
      <h2>Forgot Password</h2>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={isSubmitting}
          />
        </div>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Submitting...' : 'Reset Password'}
        </button>
      </form>
      <p>
        <Link to="/login">Return to login</Link>
      </p>
    </div>
  );
}

export default ForgotPassword;