import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface StaffMember {
  id: string;
  firstName: string;
  lastName: string;
  employeeNumber: string;
  position: string;
  isActive: boolean;
}

export interface CreateStaffRequest {
  firstName: string;
  lastName: string;
  employeeNumber: string;
  position: string;
}

export function useStaffList() {
  return useQuery({
    queryKey: ['staff'],
    queryFn: () => apiClient.get<StaffMember[]>('/staff'),
  });
}

export function useCreateStaff() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateStaffRequest) => apiClient.post<StaffMember>('/staff', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['staff'] }),
  });
}
