export interface AppDto {
  id: string;
  name: string;
  url: string;
  isActive: boolean;
  apiKey: string;
}

export interface DependencyDetail {
  status: string;
  description?: string;
  durationMs?: number;
}

export interface HealthCheckLogDto {
  id: string;
  appId?: string;
  appName?: string;
  status: string;
  totalDuration?: number;
  durationMs?: number; // Alternatif adlandırma için
  systemCpuUsage: number;
  appCpuUsage: number;
  systemRamUsage: number;
  appRamUsage: number;
  freeDiskGb: number;
  dependencyDetails: string; // JSON string
  timestamp: string;
  totalRamMb: number;
  totalCpuPercentage: number;
  totalDiskGb: number;
  totalCpuCores: number;
}

export interface IncidentDto {
  id: string;
  appId: string;
  appName: string;
  failedComponent: string;
  errorMessage: string;
  startedAt: string;
  resolvedAt: string | null;
}

export interface AiInsightDto {
  id: string;
  appName: string;
  message: string;
  evidence: string;
  insightType: string;
  isResolved: boolean;
  createdAt: string;
}
