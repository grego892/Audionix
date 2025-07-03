// src/components/Dashboard.jsx
import React, { useContext } from 'react';
import { AuthContext } from '../contexts/AuthContext';

function Dashboard() {
  const { user } = useContext(AuthContext);

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Welcome, {user?.name}! This is your dashboard.</p>
      <p>This page is only accessible to authenticated users.</p>
    </div>
  );
}

export default Dashboard;