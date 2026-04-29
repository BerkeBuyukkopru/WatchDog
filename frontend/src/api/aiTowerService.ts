import axiosClient from './axiosClient';
import type { AiInsight, AiProvider } from '../types/ai-tower.types';

export const aiTowerService = {
  getInsights: async (appId?: string, limit: number = 5): Promise<AiInsight[]> => {
    const response = await axiosClient.get<AiInsight[]>(`/api/Insights`, {
      params: { appId, limit }
    });
    return response.data;
  },

  resolveInsight: async (id: string): Promise<void> => {
    await axiosClient.patch(`/api/Insights/${id}/resolve`);
  },

  getProviders: async (): Promise<AiProvider[]> => {
    const response = await axiosClient.get<AiProvider[]>('/api/AiProviders');
    return response.data;
  },

  updateProvider: async (id: string, data: Partial<AiProvider>): Promise<void> => {
    await axiosClient.put(`/api/AiProviders/${id}`, data);
  },

  toggleProvider: async (id: string): Promise<void> => {
    // Backend'deki yeni PATCH endpoint'ini çağırıyoruz
    await axiosClient.patch(`/api/AiProviders/${id}/set-active`);
  },

  getApps: async (): Promise<any[]> => {
    const response = await axiosClient.get<any[]>('/api/Apps');
    return response.data;
  },

  setAppProvider: async (appId: string, providerId: string): Promise<void> => {
    await axiosClient.put(`/api/Apps/${appId}/ai-provider/${providerId}`);
  }
};
