import React, { useContext } from 'react';
import { AuthContext } from '../contexts/AuthContext';

function Profile() {
  const { user } = useContext(AuthContext);

  return (
    <div>
      <h1>User Profile</h1>
      {user && (
        <div>
          <p><strong>Name:</strong> {user.name}</p>
          <p><strong>Email:</strong> {user.email}</p>
        </div>
      )}
    </div>
  );
}

export default Profile;