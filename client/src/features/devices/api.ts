import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface Device {
  id: string;
  deviceName: string;
  hardwareId: string;
  firmwareVersion: string | null;
  isActive: boolean;
  registeredAtUtc: string;
  location: string | null;
  lastHeartbeatAtUtc: string | null;
  isOnline: boolean;
}

export interface RegisterDeviceRequest {
  deviceName: string;
  hardwareId: string;
  location?: string;
}

export interface RegisterDeviceResponse {
  deviceId: string;
  authToken: string;
  deviceName: string;
  hardwareId: string;
}

export function useDevices() {
  return useQuery({
    queryKey: ['devices'],
    queryFn: () => apiClient.get<Device[]>('/devices'),
    refetchInterval: 30_000,
  });
}

export function useRegisterDevice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: RegisterDeviceRequest) =>
      apiClient.post<RegisterDeviceResponse>('/devices/register', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['devices'] }),
  });
}
