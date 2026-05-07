import type { MonitoredApp } from './ai-tower.types';
export type { MonitoredApp };

export interface AdminUser {
  id: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
  deletedAt?: string;
  deletedBy?: string;
  allowedAppIds: string[];
  allowedApps?: MonitoredApp[];
  password?: string;
}

export interface CreateAdminRequest {
  username: string;
  email: string;
  password?: string;
  role: string;
  allowedAppIds: string[];
}

export interface UpdateAdminRequest {
  id: string;
  username: string;
  email: string;
  role: string;
  newPassword?: string;
  allowedAppIds: string[];
}
