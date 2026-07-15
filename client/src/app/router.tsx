import { createBrowserRouter, Navigate } from 'react-router-dom';
import { RootLayout } from './RootLayout';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { DashboardPage } from '../features/auth/pages/DashboardPage';
import { AdminPage } from '../features/auth/pages/AdminPage';
import { StudentsListPage } from '../features/students/pages/StudentsListPage';
import { StudentCreatePage } from '../features/students/pages/StudentCreatePage';
import { StudentDetailPage } from '../features/students/pages/StudentDetailPage';
import { StaffListPage } from '../features/staff/pages/StaffListPage';
import { ClassesPage } from '../features/classes/pages/ClassesPage';
import { AcademicCalendarPage } from '../features/academic-calendar/pages/AcademicCalendarPage';
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
      {
        path: '/students',
        element: (
          <ProtectedRoute>
            <StudentsListPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/students/new',
        element: (
          <ProtectedRoute requireRole="Admin">
            <StudentCreatePage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/students/:id',
        element: (
          <ProtectedRoute>
            <StudentDetailPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/staff',
        element: (
          <ProtectedRoute requireRole="Admin">
            <StaffListPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/classes',
        element: (
          <ProtectedRoute>
            <ClassesPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/academic-calendar',
        element: (
          <ProtectedRoute requireRole="Admin">
            <AcademicCalendarPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);
