import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../shared/lib/apiClient';

export interface NotificationLog {
  id: string;
  studentId: string;
  guardianId: string;
  channel: 'Email';
  triggerReason: 'Absence';
  status: 'Sent' | 'Failed';
  sentAtUtc: string;
}

export function useNotificationLogs() {
  return useQuery({
    queryKey: ['notification-logs'],
    queryFn: () => apiClient.get<NotificationLog[]>('/notifications/logs'),
  });
}
