import { createBrowserRouter, Navigate } from 'react-router-dom';
import { RootLayout } from './RootLayout';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { DashboardPage } from '../features/auth/pages/DashboardPage';
import { AdminPage } from '../features/auth/pages/AdminPage';
import { ProtectedRoute } from '../shared/components/ProtectedRoute';

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      { path: '/', element: <Navigate to="/dashboard" replace /> },
      { path: '/login', element: <LoginPage /> },
      {
        path: '/dashboard',
        element: (
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin',
        element: (
          <ProtectedRoute requireRole="Admin">
            <AdminPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);
