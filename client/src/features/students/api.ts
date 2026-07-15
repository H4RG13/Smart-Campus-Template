import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface Student {
  id: string;
  firstName: string;
  lastName: string;
  studentNumber: string;
  classId: string;
  dateOfBirth: string;
  rfidTagId: string | null;
  isActive: boolean;
}

export interface CreateStudentRequest {
  firstName: string;
  lastName: string;
  studentNumber: string;
  classId: string;
  dateOfBirth: string;
}

export function useStudents() {
  return useQuery({
    queryKey: ['students'],
    queryFn: () => apiClient.get<Student[]>('/students'),
  });
}

export function useStudent(id: string | undefined) {
  return useQuery({
    queryKey: ['students', id],
    queryFn: () => apiClient.get<Student>(`/students/${id}`),
    enabled: !!id,
  });
}

export function useCreateStudent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateStudentRequest) => apiClient.post<Student>('/students', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['students'] }),
  });
}
