import axios from 'axios';

interface ApiErrorResponse {
  message?: string;
  Message?: string;
}

export const getApiErrorMessage = (error: unknown, fallback: string): string => {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    return error.response?.data?.message ?? error.response?.data?.Message ?? fallback;
  }

  return fallback;
};
