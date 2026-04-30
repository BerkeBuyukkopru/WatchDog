import axios from 'axios';

const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5226',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor
axiosClient.interceptors.request.use(
  (config) => {
    // Check if token exists in localStorage
    const token = localStorage.getItem('watchdog_token');
    
    // If token exists, append it to the Authorization header
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor
axiosClient.interceptors.response.use(
  (response) => {
    // Return the response as is if successful
    return response;
  },
  (error) => {
    // Check for 401 Unauthorized error
    if (error.response && error.response.status === 401) {
      // Clear token from localStorage
      localStorage.removeItem('watchdog_token');
      localStorage.removeItem('user'); // Also clear user data if any
      
      // Redirect to login page immediately
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

export default axiosClient;
