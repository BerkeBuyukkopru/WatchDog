export interface AiProviderDetail {
  id: string;
  name: string;
  modelName: string;
  apiUrl: string;
  apiKey?: string; // Masked on fetch, provided on create/update
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
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
