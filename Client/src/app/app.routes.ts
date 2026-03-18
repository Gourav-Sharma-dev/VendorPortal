import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Default redirect
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Auth routes (public — redirect away if already logged in)
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login',          loadComponent: () => import('./pages/auth/login/login.component').then(m => m.LoginComponent) },
      { path: 'register',       loadComponent: () => import('./pages/auth/register/register.component').then(m => m.RegisterComponent) },
      { path: 'reset-password', loadComponent: () => import('./pages/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
    ]
  },

  // Protected shell — all authenticated pages live here
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },

      // Vendor role
      { path: 'vendor/register',  canActivate: [roleGuard('Vendor')], loadComponent: () => import('./pages/vendor/vendor-register/vendor-register.component').then(m => m.VendorRegisterComponent) },
      { path: 'vendor/dashboard', canActivate: [roleGuard('Vendor')], loadComponent: () => import('./pages/vendor/vendor-dashboard/vendor-dashboard.component').then(m => m.VendorDashboardComponent) },

      // Admin / Procurement / Finance
      { path: 'vendors',     canActivate: [roleGuard('Admin', 'Procurement', 'Finance')], loadComponent: () => import('./pages/vendor-list/vendor-list.component').then(m => m.VendorListComponent) },
      { path: 'vendors/:id', canActivate: [roleGuard('Admin', 'Procurement', 'Finance')], loadComponent: () => import('./pages/vendor-detail/vendor-detail.component').then(m => m.VendorDetailComponent) },
      { path: 'approval',    canActivate: [roleGuard('Admin', 'Procurement', 'Finance')], loadComponent: () => import('./pages/approval-queue/approval-queue.component').then(m => m.ApprovalQueueComponent) },
      { path: 'reports',     canActivate: [roleGuard('Admin', 'Procurement', 'Finance')], loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent) },

      // Admin only
      { path: 'admin/users', canActivate: [roleGuard('Admin')], loadComponent: () => import('./pages/admin/user-management/user-management.component').then(m => m.UserManagementComponent) },
      { path: 'admin/audit', canActivate: [roleGuard('Admin')], loadComponent: () => import('./pages/admin/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent) },

      { path: '**', redirectTo: 'dashboard' }
    ]
  },

  // Catch-all fallback
  { path: '**', redirectTo: 'auth/login' }
];

