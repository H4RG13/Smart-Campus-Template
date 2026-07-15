import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface SchoolClass {
  id: string;
  name: string;
  academicTermId: string;
}

export interface CreateClassRequest {
  name: string;
  academicTermId: string;
}

export function useClasses() {
  return useQuery({
    queryKey: ['classes'],
    queryFn: () => apiClient.get<SchoolClass[]>('/classes'),
  });
}

export function useCreateClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateClassRequest) => apiClient.post<SchoolClass>('/classes', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['classes'] }),
  });
}
