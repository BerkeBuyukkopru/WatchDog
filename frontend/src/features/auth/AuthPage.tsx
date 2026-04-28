import React, { useState } from 'react';
import { LoginForm } from './components/LoginForm';
import { ForgotPasswordForm } from './components/ForgotPasswordForm';
import { ResetPasswordForm } from './components/ResetPasswordForm';
import { Activity } from 'lucide-react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

type AuthState = 'LOGIN' | 'FORGOT_PASSWORD' | 'RESET_PASSWORD';

export const AuthPage: React.FC = () => {
  const { isAuthenticated, user } = useAuth();
  const [authState, setAuthState] = useState<AuthState>('LOGIN');
  const [resetEmail, setResetEmail] = useState<string>('');

  // If already authenticated, redirect based on role
  if (isAuthenticated && user) {
    const isAdmin = user.role.toLowerCase() === 'admin';
    const isSuperAdmin = user.role.toLowerCase() === 'superadmin';

    if (isSuperAdmin) {
      return <Navigate to="/management" replace />;
    }
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="min-h-screen bg-[#0A0A0B] flex flex-col items-center justify-center p-4 sm:p-8 relative overflow-hidden">
      {/* Decorative background elements */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-emerald-500/10 rounded-full blur-[120px] pointer-events-none"></div>
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-indigo-500/10 rounded-full blur-[120px] pointer-events-none"></div>

      {/* Main Container */}
      <div className="w-full max-w-md z-10">
        {/* Logo Area */}
        <div className="flex justify-center mb-8">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-emerald-400 to-emerald-600 flex items-center justify-center shadow-lg shadow-emerald-500/30">
            <Activity size={32} className="text-white" />
          </div>
        </div>

        {/* Glassmorphism Card */}
        <div className="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-8 shadow-2xl relative overflow-hidden">
          {/* Subtle gradient border top */}
          <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-emerald-500/50 to-transparent"></div>

          {/* Render the appropriate state */}
          <div className="transition-all duration-300">
            {authState === 'LOGIN' && (
              <LoginForm 
                onForgotPassword={() => setAuthState('FORGOT_PASSWORD')} 
              />
            )}

            {authState === 'FORGOT_PASSWORD' && (
              <ForgotPasswordForm 
                onBackToLogin={() => setAuthState('LOGIN')}
                onSuccess={(email) => {
                  setResetEmail(email);
                  setAuthState('RESET_PASSWORD');
                }}
              />
            )}

            {authState === 'RESET_PASSWORD' && (
              <ResetPasswordForm 
                email={resetEmail}
                onBackToLogin={() => setAuthState('LOGIN')} 
              />
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="text-center mt-8 text-slate-500 text-sm">
          &copy; {new Date().getFullYear()} WatchDog Systems. Tüm hakları saklıdır.
        </div>
      </div>
    </div>
  );
};
