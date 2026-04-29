import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthPage } from '../features/auth/AuthPage';
import { ProtectedRoute } from '../components/ProtectedRoute';
import SuperAdminLayout from '../layouts/SuperAdminLayout';
import AdminLayout from '../layouts/AdminLayout';
import DashboardView from '../features/dashboard/DashboardView';

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
          <Route index element={
            <div className="h-full flex items-center justify-center min-h-[400px]">
              <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg w-full max-w-2xl mx-auto shadow-xl">
                Global Durum (Geliştirici B Yapacak)
              </div>
            </div>
          } />

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

          <Route path="apps" element={
            <div className="h-full flex items-center justify-center min-h-[400px]">
              <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg w-full max-w-2xl mx-auto shadow-xl">
                İzlenen Uygulamalar (Geliştirici B Yapacak)
              </div>
            </div>
          } />

          <Route path="settings" element={
            <div className="h-full flex items-center justify-center min-h-[400px]">
              <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg w-full max-w-2xl mx-auto shadow-xl">
                Sistem Ayarları (Geliştirici B Yapacak)
              </div>
            </div>
          } />
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRouter;
