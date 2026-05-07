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
  },

  exportHistory: async (appId: string, days: number, appName?: string): Promise<void> => {
    const response = await axiosClient.get(`/api/apps/${appId}/export?days=${days}`, {
      responseType: 'blob'
    });
    
    // Sunucudan gelen dosya ismini yakalamaya çalış (Content-Disposition: attachment; filename="...")
    const contentDisposition = response.headers['content-disposition'];
    let fileName = '';
    
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (fileNameMatch != null && fileNameMatch[1]) {
        fileName = fileNameMatch[1].replace(/['"]/g, '');
      }
    }

    // Eğer sunucudan isim gelmediyse veya okunmadıysa yedek isim oluştur
    if (!fileName) {
      const cleanAppName = (appName || appId.substring(0, 8)).replace(/\s+/g, '_');
      fileName = `${cleanAppName}_${days}_Gunluk_Veri_${new Date().toISOString().split('T')[0]}.csv`;
    }

    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  }
};
