import axiosClient from './axiosClient';
import type { AdminUser, MonitoredApp, CreateAdminRequest, UpdateAdminRequest } from '../types/admin.types';

const BASE_URL = '/api/admins'; // Çoğul yaptık

export const adminService = {
  getAdmins: async (): Promise<AdminUser[]> => {
    const response = await axiosClient.get(BASE_URL);
    return response.data;
  },

  getDeletedAdmins: async (): Promise<AdminUser[]> => {
    const response = await axiosClient.get(`${BASE_URL}/deleted`);
    return response.data;
  },

  getAvailableApps: async (): Promise<MonitoredApp[]> => {
    const response = await axiosClient.get('/api/apps'); // Doğru endpoint
    return response.data;
  },

  createAdmin: async (admin: CreateAdminRequest): Promise<void> => {
    // Kayıt işlemi AuthController altında
    await axiosClient.post('/api/auth/register', {
      username: admin.username,
      email: admin.email,
      password: admin.password,
      role: admin.role,
      allowedAppIds: admin.allowedAppIds
    });
  },

  updateAdmin: async (id: string, admin: Omit<UpdateAdminRequest, 'id'>): Promise<void> => {
    await axiosClient.put(BASE_URL, {
      id,
      ...admin
    });
  },

  deleteAdmin: async (id: string): Promise<void> => {
    await axiosClient.delete(`${BASE_URL}/${id}`);
  },

  restoreAdmin: async (id: string): Promise<void> => {
    await axiosClient.post(`${BASE_URL}/${id}/restore`);
  }
};
