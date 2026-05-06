import axiosClient from './axiosClient';

// Arayüz Tanımlamaları
export interface SystemConfigDto {
  criticalCpuThreshold: number;
  criticalRamThreshold: number;
  criticalLatencyThreshold: number;
  scanTimeoutSeconds: number;
  retryCount: number;
}

const CONFIG_ENDPOINT = '/api/SystemConfigurations';

export const systemConfigService = {
  // Mevcut sistem ayarlarını getir
  getConfig: async (): Promise<SystemConfigDto> => {
    const response = await axiosClient.get(CONFIG_ENDPOINT);
    return response.data;
  },

  // Sistem ayarlarını güncelle
  updateConfig: async (data: SystemConfigDto): Promise<void> => {
    await axiosClient.put(CONFIG_ENDPOINT, data);
  }
};
