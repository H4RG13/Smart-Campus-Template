import type { PropsWithChildren } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../../features/auth/store';

interface ProtectedRouteProps extends PropsWithChildren {
  requireRole?: string;
}

export function ProtectedRoute({ requireRole, children }: ProtectedRouteProps) {
  const token = useAuthStore((state) => state.token);
  const hasRole = useAuthStore((state) => state.hasRole);

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (requireRole && !hasRole(requireRole)) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}
