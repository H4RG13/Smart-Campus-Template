import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { setAuthTokenProvider } from '../../shared/lib/apiClient';

export interface AuthState {
  token: string | null;
  email: string | null;
  roles: string[];
  setSession: (session: { token: string; email: string; roles: string[] }) => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      email: null,
      roles: [],
      setSession: ({ token, email, roles }) => set({ token, email, roles }),
      logout: () => set({ token: null, email: null, roles: [] }),
      hasRole: (role) => get().roles.includes(role),
    }),
    { name: 'smartcampus-auth' },
  ),
);

setAuthTokenProvider(() => useAuthStore.getState().token);
