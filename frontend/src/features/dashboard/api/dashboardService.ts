import axiosClient from '../../../api/axiosClient';
import type { HealthCheckLogDto, AiInsightDto, IncidentDto, AppDto } from '../../../types/dashboard.types';

export const dashboardService = {
  getApps: async (): Promise<AppDto[]> => {
    const response = await axiosClient.get<AppDto[]>('/api/apps');
    return response.data;
  },

  getLatestLogs: async (count: number = 50, appId?: string): Promise<HealthCheckLogDto[]> => {
    const url = appId ? `/api/status/history?count=${count}&appId=${appId}` : `/api/status/history?count=${count}`;
    const response = await axiosClient.get<HealthCheckLogDto[]>(url);
    return response.data;
  },
  
  getInsights: async (): Promise<AiInsightDto[]> => {
    const response = await axiosClient.get<AiInsightDto[]>('/api/insights');
    return response.data;
  },

  resolveInsight: async (id: string): Promise<void> => {
    await axiosClient.patch(`/api/insights/${id}/resolve`);
  },

  getIncidents: async (appId?: string): Promise<IncidentDto[]> => {
    const url = appId ? `/api/incidents?appId=${appId}` : '/api/incidents';
    const response = await axiosClient.get<IncidentDto[]>(url);
    return response.data;
  }
};
