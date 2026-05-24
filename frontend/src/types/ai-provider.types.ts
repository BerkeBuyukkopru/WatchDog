export interface AiProviderDetail {
  id: string;
  name: string;
  modelName: string;
  apiUrl: string;
  maskedApiKey?: string;
  hasApiKey: boolean;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
  deletedAt?: string;
  deletedBy?: string;
}

export interface CreateAiProviderRequest {
  name: string;
  modelName: string;
  apiUrl: string;
  apiKey: string;
  isActive: boolean;
}

export interface UpdateAiProviderRequest {
  id: string;
  name: string;
  modelName: string;
  apiUrl: string;
  apiKey?: string; // Optional if not changing
  isActive: boolean;
}
