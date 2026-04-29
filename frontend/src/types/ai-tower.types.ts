export type InsightSeverity = 'low' | 'medium' | 'high' | 'critical';

export interface AiInsight {
  id: string;
  appName: string;
  message: string;
  evidence: string;
  insightType: string;
  isResolved: boolean;
  createdAt: string;
}

export interface AiProvider {
  id: string;
  name: string;
  modelName: string;
  apiUrl?: string;
  isActive: boolean;
  hasApiKey: boolean;
}

export interface MonitoredApp {
  id: string;
  name: string;
  healthUrl: string;
  isActive: boolean;
  activeAiProviderId?: string;
}
