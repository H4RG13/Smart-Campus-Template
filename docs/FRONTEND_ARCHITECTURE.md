# Frontend Architecture

React + TypeScript + Vite, mirroring the backend's feature-based organization so a developer can find "the Attendance feature" in the same shape on both sides of the API boundary.

## Folder structure

```
client/src/
├── app/
│   ├── App.tsx                 # Root component, providers
│   ├── router.tsx               # React Router route tree
│   ├── providers/               # QueryClientProvider, ThemeProvider, AuthProvider
│   └── store/                   # Zustand root store composition
│
├── config/
│   ├── useBranding.ts            # Fetches /api/v1/config/branding, applies theme
│   └── theme.ts                  # Tailwind CSS variable mapping from branding config
│
├── features/
│   ├── auth/
│   │   ├── api.ts                 # TanStack Query hooks calling /auth/*
│   │   ├── store.ts               # Zustand slice: current user, token
│   │   ├── components/
│   │   └── pages/
│   ├── students/
│   ├── staff/
│   ├── attendance/
│   ├── devices/
│   ├── academic-calendar/
│   ├── notifications/
│   └── reports/
│
├── shared/
│   ├── components/                # Button, Table, Modal, FormField — generic UI primitives
│   ├── hooks/
│   ├── lib/                        # apiClient (fetch wrapper), formatting utils
│   └── types/                      # Shared TS types generated/mirrored from API DTOs
│
└── main.tsx
```

**Why mirror the backend's feature slices:** the same reasoning as Section 4/9 of `ARCHITECTURE.md` — a developer working on "Attendance" end-to-end (API + UI) should navigate one conceptual folder name across both codebases, and it keeps the same segregation between school-agnostic feature code and per-school custom UI (see Custom Reports below).

## State management strategy

Two clearly separated concerns, deliberately not unified into one state library:

- **Server state → TanStack Query.** Anything that originates from the API (students, attendance records, device status) is fetched, cached, and invalidated via TanStack Query. No manually-managed `useEffect` + `useState` fetch loops, and no duplicating server data into Zustand.
- **Client/UI state → Zustand.** Current user/auth token, theme, sidebar collapsed state, in-progress form drafts not yet submitted — state that has no server counterpart.

**Why not Redux:** this app's client-state surface (auth, theme, UI toggles) is small enough that Redux's boilerplate (actions, reducers, middleware) buys nothing over Zustand's direct store API. Reaching for Redux here would be exactly the kind of unneeded ceremony `RULES.md` warns against.

## Routing

React Router, route tree organized by role-gated sections:

```
/                        → redirect based on role
/login
/dashboard                (role-aware landing: admin overview vs teacher's class view)
/students                 (Admin, Staff, Teacher)
/students/:id
/staff                    (Admin)
/attendance                (Admin, Teacher, Staff)
/devices                   (Admin, Staff)
/academic-calendar          (Admin)
/reports                   (Admin, Teacher)
/reports/custom/:reportKey  (Admin, per-school registered reports)
/settings/branding           (Admin — mostly read-only, config is env-driven)
```

Route guards check role from the `auth` Zustand slice; unauthorized access redirects to `/dashboard` rather than showing a 403 page — keeps the UX simple for a small user base per school.

## Branding & theming

On app boot, `useBranding()` fetches `/api/v1/config/branding` once (cached indefinitely for the session) and applies:
- School name/logo → header, favicon, login page
- Theme colors → CSS custom properties consumed by Tailwind (`--color-primary`, etc.), not hardcoded Tailwind color classes — this is what lets one codebase render differently per school without a code change, matching Rule #2.
- Timezone → used by shared date-formatting utilities so all displayed times are in the school's local time, while everything transmitted to/from the API stays UTC.

## Forms

React Hook Form + a shared `zodResolver`-style validation schema per feature (mirroring the backend's FluentValidation rules conceptually, not by codegen in V1 — keeping frontend/backend validation in sync is a manual discipline documented per feature, not automated, to avoid a premature codegen pipeline before the API is stable).

## Data visualization

Apache ECharts, wrapped in a small `shared/components/Chart` primitive so chart theming (colors, fonts) also derives from the branding config rather than being hardcoded per chart instantiation.

## Real-time updates

A single `useLiveAttendanceHub()` hook wraps the SignalR connection (`/hubs/live-attendance`), feeding received events into TanStack Query's cache via `queryClient.setQueryData`/`invalidateQueries` rather than a separate real-time state store — keeps "attendance data" as a single source of truth (the Query cache) regardless of whether it arrived via fetch or push.

## Custom per-school UI

Per-school bespoke report UIs live under `features/reports/custom/{reportKey}/`, registered in a small lookup map consumed by the `/reports/custom/:reportKey` route — the frontend mirror of the backend's `Reports/Custom/` isolation (Rule #9), so a school-specific screen never requires touching a shared feature's component.

## What's explicitly not in V1

- No server-side rendering (SSR) / Next.js — a school-internal admin tool has no SEO or public-content need that would justify it.
- No component library beyond Tailwind + hand-built primitives in `shared/components` — pulling in a full design system (MUI, Ant) adds a theming fight against per-school branding that plain Tailwind + CSS variables avoids.
- No i18n framework — single-language per school assumed for V1; if a school needs multi-language, that's a config-driven addition (a language setting + string tables), not a fork.
