import axiosClient from './axiosClient';
import type { LoginRequest, LoginResponse, ResetPasswordRequest } from '../types/auth.types';

interface MessageResponse {
  message?: string;
  Message?: string;
}

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await axiosClient.post<LoginResponse>('/api/Auth/login', data);
    return response.data;
  },

  forgotPassword: async (email: string): Promise<MessageResponse> => {
    const response = await axiosClient.post<MessageResponse>('/api/Auth/forgot-password', JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  },

  resetPassword: async (data: ResetPasswordRequest): Promise<MessageResponse> => {
    const response = await axiosClient.post<MessageResponse>('/api/Auth/reset-password', data);
    return response.data;
  },
};
