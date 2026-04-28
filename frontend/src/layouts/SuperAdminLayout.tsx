import { useState } from 'react';
import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { Activity, Users, Bot, AppWindow, Settings, LogOut, Menu, X } from 'lucide-react';

const SuperAdminLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const closeMobileMenu = () => setIsMobileMenuOpen(false);

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
            <div className="flex items-center gap-2 text-status-healthy">
              <div className="w-2 h-2 rounded-full bg-status-healthy animate-pulse"></div>
              <span className="text-sm font-medium hidden sm:inline">System Online</span>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <button onClick={() => navigate('/login')} className="flex items-center gap-2 text-sm text-rose-400 hover:text-rose-300 transition-colors">
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
