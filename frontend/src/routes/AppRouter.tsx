import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthPage } from '../features/auth/AuthPage';
import { ProtectedRoute } from '../components/ProtectedRoute';
import SuperAdminLayout from '../layouts/SuperAdminLayout';
import AdminLayout from '../layouts/AdminLayout';
import DashboardView from '../features/dashboard/DashboardView';
import { AdminListView } from '../features/admin-management/AdminListView';
import { AiProviderListView } from '../features/ai-providers/AiProviderListView';
import GlobalDashboard from '../features/apps/GlobalDashboard';
import AppsManagementPage from '../features/apps/AppsManagementPage';
import SystemSettingsPage from '../features/system-settings/SystemSettingsPage';

const AppRouter = () => {
  return (
    <Routes>
      {/* Root redirect */}
      <Route path="/" element={<Navigate to="/login" replace />} />

      {/* Auth */}
      <Route path="/login" element={<AuthPage />} />

      {/* Protected Routes */}
      <Route element={<ProtectedRoute allowedRoles={['Admin', 'SuperAdmin']} />}>
        {/* Live Dashboard (Phase 2) */}
        <Route path="/dashboard" element={<AdminLayout />}>
          <Route index element={<DashboardView />} />
        </Route>
      </Route>

      {/* Super Admin Management - Only for SuperAdmin */}
      <Route element={<ProtectedRoute allowedRoles={['SuperAdmin']} />}>
        <Route path="/management" element={<SuperAdminLayout />}>
          <Route index element={<GlobalDashboard />} />

          <Route path="admins" element={<AdminListView />} />

          <Route path="ai-providers" element={<AiProviderListView />} />

          <Route path="apps" element={<AppsManagementPage />} />

          <Route path="settings" element={<SystemSettingsPage />} />
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRouter;
