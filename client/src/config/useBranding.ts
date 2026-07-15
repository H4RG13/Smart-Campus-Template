import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../shared/lib/apiClient';

export interface BrandingSettings {
  schoolName: string;
  logoUrl: string;
  primaryColor: string;
  secondaryColor: string;
  timezone: string;
}

export function useBranding() {
  return useQuery({
    queryKey: ['config', 'branding'],
    queryFn: () => apiClient.get<BrandingSettings>('/config/branding'),
    staleTime: Infinity,
  });
}

export function applyBrandingTheme(branding: BrandingSettings) {
  const root = document.documentElement;
  root.style.setProperty('--color-primary', branding.primaryColor);
  root.style.setProperty('--color-secondary', branding.secondaryColor);
  document.title = branding.schoolName;
}
