import axiosClient from './axiosClient';
import type { AiProviderDetail, CreateAiProviderRequest } from '../types/ai-provider.types';

const BASE_URL = '/api/aiproviders'; // Backend ile uyumlu çoğul isim

export const aiProviderService = {
  getProviders: async (): Promise<AiProviderDetail[]> => {
    const response = await axiosClient.get(BASE_URL);
    return response.data;
  },

  getDeletedProviders: async (): Promise<AiProviderDetail[]> => {
    const response = await axiosClient.get(`${BASE_URL}/deleted`);
    return response.data;
  },

  createProvider: async (provider: CreateAiProviderRequest): Promise<void> => {
    await axiosClient.post(BASE_URL, provider);
  },

  updateProvider: async (id: string, provider: Partial<CreateAiProviderRequest>): Promise<void> => {
    await axiosClient.put(`${BASE_URL}/${id}`, provider);
  },

  deleteProvider: async (id: string): Promise<void> => {
    await axiosClient.delete(`${BASE_URL}/${id}`);
  },

  restoreProvider: async (id: string): Promise<void> => {
    await axiosClient.post(`${BASE_URL}/${id}/restore`);
  },

  toggleStatus: async (id: string): Promise<void> => {
    await axiosClient.patch(`${BASE_URL}/${id}/toggle-status`);
  }
};
