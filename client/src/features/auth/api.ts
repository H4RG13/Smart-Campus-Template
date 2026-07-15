import { useMutation } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';
import { useAuthStore } from './store';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
  roles: string[];
}

export function useLogin() {
  const setSession = useAuthStore((state) => state.setSession);

  return useMutation({
    mutationFn: (request: LoginRequest) => apiClient.post<LoginResponse>('/auth/login', request),
    onSuccess: (response) => {
      setSession({ token: response.token, email: response.email, roles: response.roles });
    },
  });
}
