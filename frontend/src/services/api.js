// src/services/api.js
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.NODE_ENV === 'production' 
    ? window.location.origin // Use same origin in production
    : 'http://localhost:8001' // Development URL
});

// Add request interceptor to include auth token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;