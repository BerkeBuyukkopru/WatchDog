export interface LoginRequest {
  username: string;
  password?: string;
}

export interface LoginResponse {
  isSuccess: boolean;
  token?: string;
  errorMessage?: string;
}

export interface ResetPasswordRequest {
  Email: string;
  ResetCode: string;
  NewPassword: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
}
