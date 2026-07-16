import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface GuardianStudentLink {
  studentId: string;
  relationship: string;
}

export interface Guardian {
  id: string;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  isActive: boolean;
  studentLinks: GuardianStudentLink[];
}

export interface CreateGuardianRequest {
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
}

export interface LinkGuardianToStudentRequest {
  studentId: string;
  relationship: string;
}

export function useGuardians() {
  return useQuery({
    queryKey: ['guardians'],
    queryFn: () => apiClient.get<Guardian[]>('/guardians'),
  });
}

export function useCreateGuardian() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateGuardianRequest) => apiClient.post<Guardian>('/guardians', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['guardians'] }),
  });
}

export function useLinkGuardianToStudent(guardianId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: LinkGuardianToStudentRequest) =>
      apiClient.post<Guardian>(`/guardians/${guardianId}/students`, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['guardians'] }),
  });
}
