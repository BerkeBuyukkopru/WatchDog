import { useState } from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Activity, Users, Bot, AppWindow, Settings, LogOut, Menu, X, User } from 'lucide-react';
import { useEffect } from 'react';

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

  const pageTitle = getPageTitle();

  // Browser sekme başlığını güncelle
  useEffect(() => {
    document.title = `Watchdog | ${pageTitle}`;
  }, [pageTitle]);

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
      <aside className="hidden md:flex w-72 bg-background border-r border-white/5 flex-col shrink-0">
        <div className="h-16 flex items-center gap-4 px-6 border-b border-white/5 shrink-0">
          <div className="w-10 h-10 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5">
            <Activity size={22} strokeWidth={2.5} />
          </div>
          <div className="flex flex-col justify-center">
            <h1 className="text-sm font-black text-white tracking-[0.25em] leading-none mb-2">WATCHDOG</h1>
            <div className="px-2 py-0.5 rounded-full text-[7px] font-black uppercase tracking-[0.1em] w-fit leading-none border text-indigo-400 border-indigo-400/30 bg-indigo-500/5">
              MANAGEMENT
            </div>
          </div>
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
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5">
                  <Activity size={18} strokeWidth={2.5} />
                </div>
                <div className="flex flex-col justify-center">
                  <h1 className="text-sm font-black text-white tracking-[0.25em] leading-none mb-1">WATCHDOG</h1>
                  <div className="px-2 py-0.5 rounded-full text-[7px] font-black uppercase tracking-[0.1em] w-fit leading-none border text-indigo-400 border-indigo-400/30 bg-indigo-500/5">
                    MANAGEMENT
                  </div>
                </div>
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
        <header className="h-16 bg-background border-b border-white/5 flex items-center justify-between px-6 shrink-0">
          <div className="flex items-center gap-6">
            <button 
              className="md:hidden text-slate-400 hover:text-white p-1"
              onClick={() => setIsMobileMenuOpen(true)}
            >
              <Menu size={20} />
            </button>
            <div className="flex items-center gap-2 sm:gap-4">
              <h1 className="text-[10px] font-black text-slate-100 uppercase tracking-widest sm:hidden truncate max-w-[120px]">
                {pageTitle}
              </h1>
              <h1 className="text-xs font-black text-slate-500 uppercase tracking-widest hidden sm:block">
                Super Admin <span className="mx-2 text-slate-800">/</span> 
                <span className="text-slate-100">{pageTitle}</span>
              </h1>
            </div>
          </div>
          <div className="flex items-center gap-4 sm:gap-8">
            {/* Kullanıcı Bilgisi */}
            <div className="flex items-center gap-2 sm:gap-3 pr-2">
              <div className="hidden sm:flex flex-col items-end">
                <span className="text-xs font-black text-white leading-none uppercase tracking-wider">{user?.username || 'Berke Buyukkopru'}</span>
                <span className="text-[9px] font-bold text-slate-500 lowercase tracking-tight mt-1">{user?.email || 'admin@watchdog.com'}</span>
              </div>
              <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-white/5 flex items-center justify-center text-slate-400 border border-white/10 shadow-lg">
                <User size={18} strokeWidth={2.5} />
              </div>
            </div>

            <button onClick={logout} className="flex items-center gap-2 text-[11px] font-black uppercase tracking-[0.15em] text-rose-500 hover:text-rose-400 transition-all active:scale-95">
              <LogOut size={16} strokeWidth={3} />
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
