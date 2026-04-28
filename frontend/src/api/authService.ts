import axiosClient from './axiosClient';
import type { LoginRequest, LoginResponse, ResetPasswordRequest } from '../types/auth.types';

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await axiosClient.post<LoginResponse>('/api/Auth/login', data);
    return response.data;
  },

  forgotPassword: async (email: string): Promise<any> => {
    const response = await axiosClient.post('/api/Auth/forgot-password', JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  },

  resetPassword: async (data: ResetPasswordRequest): Promise<any> => {
    const response = await axiosClient.post('/api/Auth/reset-password', data);
    return response.data;
  },
};
