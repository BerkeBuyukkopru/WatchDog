import { useState } from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Activity, Users, Bot, AppWindow, Settings, LogOut, Menu, X, User, Globe } from 'lucide-react';

const SuperAdminLayout = () => {
  const { logout, user } = useAuth();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const closeMobileMenu = () => setIsMobileMenuOpen(false);

  // URL'ye göre sayfa başlığını belirleyen yardımcı fonksiyon
  const getPageTitle = () => {
    const path = location.pathname;
    if (path.includes('/admins')) return 'Admin Yönetimi';
    if (path.includes('/ai-providers')) return 'AI Sağlayıcıları';
    if (path.includes('/apps')) return 'İzlenen Uygulamalar';
    if (path.includes('/settings')) return 'Sistem Ayarları';
    return 'Global Durum';
  };

  const NavLinks = () => (
    <>
      <Link to="/management" onClick={closeMobileMenu} className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${location.pathname === '/management' ? 'bg-slate-800 text-white' : 'hover:bg-slate-800 text-slate-300 hover:text-white'}`}>
        <Activity size={18} />
        <span>Global Durum</span>
      </Link>
      <Link to="/management/admins" onClick={closeMobileMenu} className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${location.pathname.includes('/admins') ? 'bg-slate-800 text-white' : 'hover:bg-slate-800 text-slate-300 hover:text-white'}`}>
        <Users size={18} />
        <span>Admin Yönetimi</span>
      </Link>
      <Link to="/management/ai-providers" onClick={closeMobileMenu} className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${location.pathname.includes('/ai-providers') ? 'bg-slate-800 text-white' : 'hover:bg-slate-800 text-slate-300 hover:text-white'}`}>
        <Bot size={18} />
        <span>AI Sağlayıcıları</span>
      </Link>
      <Link to="/management/apps" onClick={closeMobileMenu} className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${location.pathname.includes('/apps') ? 'bg-slate-800 text-white' : 'hover:bg-slate-800 text-slate-300 hover:text-white'}`}>
        <AppWindow size={18} />
        <span>İzlenen Uygulamalar</span>
      </Link>
      <Link to="/management/settings" onClick={closeMobileMenu} className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${location.pathname.includes('/settings') ? 'bg-slate-800 text-white' : 'hover:bg-slate-800 text-slate-300 hover:text-white'}`}>
        <Settings size={18} />
        <span>Sistem Ayarları</span>
      </Link>
    </>
  );

  return (
    <div className="min-h-screen bg-background-darker flex">
      {/* Desktop Sidebar */}
      <aside className="hidden md:flex w-64 bg-background border-r border-slate-800 flex-col shrink-0">
        <div className="h-16 flex flex-col justify-center px-6 border-b border-slate-800 shrink-0">
          <h1 className="text-lg font-bold text-accent tracking-wider leading-none mb-1">WATCHDOG</h1>
          <p className="text-[11px] text-slate-400 leading-none">Super Admin</p>
        </div>
        <nav className="flex-1 py-4 flex flex-col gap-2 px-3 overflow-y-auto custom-scrollbar">
          <NavLinks />
        </nav>
      </aside>

      {/* Mobile Drawer */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 flex md:hidden">
          <div className="fixed inset-0 bg-black/60 backdrop-blur-sm" onClick={closeMobileMenu}></div>
          <aside className="relative w-64 bg-background border-r border-slate-800 flex flex-col h-full shadow-2xl">
            <div className="h-16 flex items-center justify-between px-6 border-b border-slate-800 shrink-0">
              <div className="flex flex-col justify-center">
                <h1 className="text-lg font-bold text-accent tracking-wider leading-none mb-1">WATCHDOG</h1>
                <p className="text-[11px] text-slate-400 leading-none">Super Admin</p>
              </div>
              <button onClick={closeMobileMenu} className="text-slate-400 hover:text-white">
                <X size={20} />
              </button>
            </div>
            <nav className="flex-1 py-4 flex flex-col gap-2 px-3 overflow-y-auto custom-scrollbar">
              <NavLinks />
            </nav>
          </aside>
        </div>
      )}

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Header */}
        <header className="h-16 bg-background border-b border-slate-800 flex items-center justify-between px-4 sm:px-6 shrink-0">
          <div className="flex items-center gap-3">
            <button 
              className="md:hidden text-slate-400 hover:text-white p-1"
              onClick={() => setIsMobileMenuOpen(true)}
            >
              <Menu size={20} />
            </button>
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-full bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20">
                <Globe size={18} />
              </div>
              <h1 className="text-sm font-black text-slate-200 uppercase tracking-widest hidden sm:block">
                {getPageTitle()}
              </h1>
            </div>
          </div>
          <div className="flex items-center gap-6">
            {/* Kullanıcı Bilgisi */}
            <div className="flex items-center gap-3 border-r border-slate-800 pr-6">
              <div className="flex flex-col items-end">
                <span className="text-xs font-black text-slate-100 leading-none">{user?.username || 'SuperAdmin'}</span>
                <span className="text-[9px] font-bold text-slate-500 lowercase tracking-tight mt-1">{user?.email || 'admin@watchdog.com'}</span>
              </div>
              <div className="w-9 h-9 rounded-xl bg-slate-800 flex items-center justify-center text-slate-400 border border-slate-700">
                <User size={18} />
              </div>
            </div>

            <button onClick={logout} className="flex items-center gap-2 text-sm text-rose-400 hover:text-rose-300 transition-colors font-bold uppercase tracking-wider">
              <LogOut size={16} />
              <span className="hidden sm:inline">Çıkış</span>
            </button>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-auto p-4 sm:p-6 custom-scrollbar">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default SuperAdminLayout;
