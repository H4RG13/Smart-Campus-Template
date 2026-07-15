import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface Term {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface CreateTermRequest {
  name: string;
  startDate: string;
  endDate: string;
}

export type CalendarExceptionType = 'Holiday' | 'HalfDay' | 'SpecialSchedule';

export interface AddExceptionRequest {
  date: string;
  type: CalendarExceptionType;
  description?: string;
}

export function useTerms() {
  return useQuery({
    queryKey: ['academic-terms'],
    queryFn: () => apiClient.get<Term[]>('/academic-terms'),
  });
}

export function useCreateTerm() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateTermRequest) => apiClient.post<Term>('/academic-terms', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['academic-terms'] }),
  });
}

export function useAddException(termId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: AddExceptionRequest) =>
      apiClient.post(`/academic-terms/${termId}/exceptions`, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['academic-terms'] }),
  });
}
