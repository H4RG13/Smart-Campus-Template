import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export type AttendanceStatus = 'OnTime' | 'Late' | 'Absent' | 'ExcusedAbsence';

export interface AttendanceRecord {
  id: string;
  studentId: string;
  attendanceDate: string;
  checkInAtUtc: string | null;
  status: AttendanceStatus;
  recordedByUserId: string | null;
  notes: string | null;
  createdAtUtc: string;
}

export interface RecordAttendanceRequest {
  studentId: string;
  attendanceDate: string;
  checkInTime?: string;
  status?: AttendanceStatus;
  notes?: string;
}

export interface CorrectAttendanceRequest {
  status: AttendanceStatus;
  notes?: string;
}

export interface AttendanceDaySummary {
  date: string;
  onTimeCount: number;
  lateCount: number;
  absentCount: number;
  excusedAbsenceCount: number;
}

export function useAttendanceRecords(params: { studentId?: string; fromDate?: string; toDate?: string }) {
  const query = new URLSearchParams();
  if (params.studentId) query.set('studentId', params.studentId);
  if (params.fromDate) query.set('fromDate', params.fromDate);
  if (params.toDate) query.set('toDate', params.toDate);

  return useQuery({
    queryKey: ['attendance-records', params],
    queryFn: () => apiClient.get<AttendanceRecord[]>(`/attendance-records?${query.toString()}`),
  });
}

export function useAttendanceSummary(fromDate: string, toDate: string) {
  return useQuery({
    queryKey: ['attendance-records', 'summary', fromDate, toDate],
    queryFn: () =>
      apiClient.get<AttendanceDaySummary[]>(`/attendance-records/summary?fromDate=${fromDate}&toDate=${toDate}`),
  });
}

export function useRecordAttendance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: RecordAttendanceRequest) =>
      apiClient.post<AttendanceRecord>('/attendance-records', request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attendance-records'] }),
  });
}

export function useCorrectAttendance(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CorrectAttendanceRequest) =>
      apiClient.put<AttendanceRecord>(`/attendance-records/${id}`, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attendance-records'] }),
  });
}
