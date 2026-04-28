import { Outlet, useNavigate } from 'react-router-dom';
import { LogOut, Activity } from 'lucide-react';

const AdminLayout = () => {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-background-darker flex flex-col">
      {/* Header */}
      <header className="h-16 bg-background border-b border-slate-800 flex items-center justify-between px-4 sm:px-6 shrink-0">
        <div className="flex items-center gap-3 sm:gap-6">
          <div className="flex items-center gap-2">
            <Activity className="text-accent" size={20} />
            <h1 className="text-lg sm:text-xl font-bold text-slate-100 tracking-wider">WATCHDOG</h1>
          </div>
          <div className="flex items-center gap-2 px-2 sm:px-3 py-1 bg-status-healthy/10 rounded-full border border-status-healthy/20">
            <div className="w-2 h-2 rounded-full bg-status-healthy animate-pulse"></div>
            <span className="text-[10px] sm:text-xs font-semibold text-status-healthy uppercase tracking-wider hidden sm:inline">WatchDog Active</span>
            <span className="text-[10px] font-semibold text-status-healthy uppercase tracking-wider sm:hidden">Active</span>
          </div>
        </div>
        
        <div className="flex items-center gap-4">
          <button onClick={() => navigate('/login')} className="flex items-center gap-2 text-sm text-rose-400 hover:text-rose-300 transition-colors">
            <LogOut size={16} />
            <span className="hidden sm:inline">Çıkış</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-auto custom-scrollbar">
        <Outlet />
      </main>
    </div>
  );
};

export default AdminLayout;
