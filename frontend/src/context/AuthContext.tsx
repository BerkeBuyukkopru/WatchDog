import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';

import type { User } from '../types/auth.types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Initialize auth state from localStorage on first render
  useEffect(() => {
    const storedToken = sessionStorage.getItem('watchdog_token');
    if (storedToken) {
      try {
        const decodedToken = jwtDecode<any>(storedToken);
        
        // Basic check if token is expired
        const currentTime = Date.now() / 1000;
        if (decodedToken.exp && decodedToken.exp < currentTime) {
          // Token expired
          sessionStorage.removeItem('watchdog_token');
        } else {
          // Extract user info from token (adjust keys based on actual backend JWT claims)
          const rawRole = decodedToken.role || decodedToken.roles || decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
          const role = Array.isArray(rawRole) ? rawRole[0] : rawRole;
          
          const userData: User = {
            id: decodedToken.sub || decodedToken.nameid || '',
            username: decodedToken.unique_name || decodedToken.name || '',
            email: decodedToken.email || '',
            role: role,
          };
          
          setToken(storedToken);
          setUser(userData);
          setIsAuthenticated(true);
        }
      } catch (error) {
        console.error('Invalid token found in localStorage', error);
        sessionStorage.removeItem('watchdog_token');
      }
    }
    setIsLoading(false);
  }, []);

  const login = (newToken: string) => {
    try {
      const decodedToken = jwtDecode<any>(newToken);
      
      const rawRole = decodedToken.role || decodedToken.roles || decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
      const role = Array.isArray(rawRole) ? rawRole[0] : rawRole;

      const userData: User = {
        id: decodedToken.sub || decodedToken.nameid || '',
        username: decodedToken.unique_name || decodedToken.name || '',
        email: decodedToken.email || '',
        role: role,
      };

      sessionStorage.setItem('watchdog_token', newToken);
      setToken(newToken);
      setUser(userData);
      setIsAuthenticated(true);
    } catch (error) {
      console.error('Failed to decode token on login', error);
    }
  };

  const logout = () => {
    sessionStorage.removeItem('watchdog_token');
    setToken(null);
    setUser(null);
    setIsAuthenticated(false);
    window.location.href = '/login';
  };

  if (isLoading) {
    // Return a loader or null while checking local storage to prevent unwanted redirects
    return <div className="min-h-screen bg-[#0A0A0B] flex items-center justify-center">
      <div className="w-8 h-8 border-4 border-emerald-500/20 border-t-emerald-500 rounded-full animate-spin"></div>
    </div>;
  }

  return (
    <AuthContext.Provider value={{ user, token, isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
