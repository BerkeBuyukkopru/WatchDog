import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthPage } from '../features/auth/AuthPage';
import { ProtectedRoute } from '../components/ProtectedRoute';
import SuperAdminLayout from '../layouts/SuperAdminLayout';
import AdminLayout from '../layouts/AdminLayout';
import DashboardView from '../features/dashboard/DashboardView';

// Phase 3 Management Pages (Berke's Area)
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

          <Route path="admins" element={
            <div className="h-full flex items-center justify-center min-h-[400px]">
              <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg w-full max-w-2xl mx-auto shadow-xl">
                Admin Yönetimi (Geliştirici B Yapacak)
              </div>
            </div>
          } />

          <Route path="ai-providers" element={
            <div className="h-full flex items-center justify-center min-h-[400px]">
              <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg w-full max-w-2xl mx-auto shadow-xl">
                AI Sağlayıcıları (Geliştirici B Yapacak)
              </div>
            </div>
          } />

          <Route path="apps" element={<AppsManagementPage />} />

          <Route path="settings" element={<SystemSettingsPage />} />
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRouter;
