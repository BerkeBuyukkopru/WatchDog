import { Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, Activity } from 'lucide-react';
import { useState } from 'react';

const AdminLayout = () => {
  const { logout } = useAuth();
  const [apiError, setApiError] = useState<boolean>(false);

  return (
    <div className="min-h-screen bg-background-darker flex flex-col">
      {/* Header */}
      <header className="h-16 bg-background border-b border-slate-800 flex items-center justify-between px-4 sm:px-6 shrink-0">
        <div className="flex items-center gap-3 sm:gap-6">
          <div className="flex items-center gap-2">
            <Activity className="text-accent" size={20} />
            <h1 className="text-lg sm:text-xl font-bold text-slate-100 tracking-wider">WATCHDOG</h1>
          </div>
          <div className={`flex items-center gap-2 px-2 sm:px-3 py-1 rounded-full border ${apiError ? 'bg-rose-500/10 border-rose-500/20' : 'bg-status-healthy/10 border-status-healthy/20'}`}>
            <div className={`w-2 h-2 rounded-full ${apiError ? 'bg-rose-500' : 'bg-status-healthy animate-pulse'}`}></div>
            <span className={`text-[10px] sm:text-xs font-semibold uppercase tracking-wider hidden sm:inline ${apiError ? 'text-rose-500' : 'text-status-healthy'}`}>
              {apiError ? 'WATCHDOG OFFLINE' : 'WATCHDOG ONLINE'}
            </span>
            <span className={`text-[10px] font-semibold uppercase tracking-wider sm:hidden ${apiError ? 'text-rose-500' : 'text-status-healthy'}`}>
              {apiError ? 'OFFLINE' : 'ONLINE'}
            </span>
          </div>
        </div>
        
        <div className="flex items-center gap-4">
          <button onClick={logout} className="flex items-center gap-2 text-sm text-rose-400 hover:text-rose-300 transition-colors">
            <LogOut size={16} />
            <span className="hidden sm:inline">Çıkış</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-auto custom-scrollbar">
        <Outlet context={{ setApiError }} />
      </main>
    </div>
  );
};

export default AdminLayout;
