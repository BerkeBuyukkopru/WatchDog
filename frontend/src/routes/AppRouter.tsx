import { Routes, Route, Navigate } from 'react-router-dom';
import SuperAdminLayout from '../layouts/SuperAdminLayout';
import AdminLayout from '../layouts/AdminLayout';

const AppRouter = () => {
  return (
    <Routes>
      {/* Root redirect */}
      <Route path="/" element={<Navigate to="/login" replace />} />
      
      {/* Auth */}
      <Route path="/login" element={
        <div className="min-h-screen bg-background-darker flex items-center justify-center p-4">
          <div className="w-full max-w-md text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg shadow-xl">
            Login Sayfası (Geliştirici B Yapacak)
          </div>
        </div>
      } />
      
      {/* Live Dashboard (Phase 2) */}
      <Route path="/dashboard" element={<AdminLayout />}>
        <Route index element={
          <div className="h-full flex items-center justify-center p-4 sm:p-6">
            <div className="text-white p-8 sm:p-10 text-center text-lg sm:text-xl bg-background border border-slate-800 rounded-lg shadow-xl w-full max-w-2xl">
              Admin İzleme Ekranı (Faz 2'de Yapılacak)
            </div>
          </div>
        } />
      </Route>
      
      {/* Super Admin Management */}
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
    </Routes>
  );
};

export default AppRouter;
