import axiosClient from './axiosClient';

// Arayüz Tanımlamaları - Backend ile tam uyumlu
export interface AppDto {
  id: string;
  name: string;
  healthUrl: string;
  pollingIntervalSeconds: number;
  activeAiProviderId: string;
  isActive: boolean;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
  deletedAt?: string;
  deletedBy?: string;
}

export interface CreateAppCommand {
  name: string;
  healthUrl: string;
  pollingIntervalSeconds: number;
  activeAiProviderId?: string;
}

export interface UpdateAppCommand {
  id: string;
  name: string;
  healthUrl: string;
  pollingIntervalSeconds: number;
  activeAiProviderId?: string;
  isActive: boolean;
}

const APPS_ENDPOINT = '/api/Apps';

export const appsService = {
  // Tüm uygulamaları getir
  getAllApps: async (): Promise<AppDto[]> => {
    const response = await axiosClient.get(APPS_ENDPOINT);
    return response.data;
  },

  // Tek bir uygulama getir
  getAppById: async (id: string): Promise<AppDto> => {
    const response = await axiosClient.get(`${APPS_ENDPOINT}/${id}`);
    return response.data;
  },

  // Yeni uygulama ekle
  createApp: async (data: CreateAppCommand): Promise<string> => {
    const response = await axiosClient.post(APPS_ENDPOINT, data);
    return response.data;
  },

  // Mevcut uygulamayı güncelle
  updateApp: async (id: string, data: UpdateAppCommand): Promise<void> => {
    await axiosClient.put(`${APPS_ENDPOINT}/${id}`, data);
  },

  // Uygulamayı sil
  deleteApp: async (id: string): Promise<void> => {
    await axiosClient.delete(`${APPS_ENDPOINT}/${id}`);
  },

  // Uygulama izleme durumunu değiştir (Aktif/Pasif Toggle)
  toggleAppStatus: async (id: string): Promise<void> => {
    await axiosClient.put(`${APPS_ENDPOINT}/${id}/toggle-status`);
  },

  // Uygulama için manuel AI analizi tetikle
  triggerManualAnalysis: async (id: string): Promise<void> => {
    await axiosClient.post(`${APPS_ENDPOINT}/${id}/analyze`);
  },

  // Silinmiş uygulamaları getir
  getDeletedApps: async (): Promise<AppDto[]> => {
    const response = await axiosClient.get(`${APPS_ENDPOINT}/deleted`);
    return response.data;
  },

  // Uygulamayı geri yükle
  restoreApp: async (id: string): Promise<void> => {
    await axiosClient.post(`${APPS_ENDPOINT}/${id}/restore`);
  }
};
