import { Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, Activity } from 'lucide-react';
import { useState } from 'react';

const AdminLayout = () => {
  const { logout, user } = useAuth();
  const [apiError, setApiError] = useState<boolean>(false);

  return (
    <div className="min-h-screen bg-background-darker flex flex-col">
      {/* Header */}
      <header className="h-16 bg-background border-b border-white/5 flex items-center justify-between px-6 shrink-0">
        <div className="flex items-center gap-6">
          <div className="flex items-center gap-4">
            <div className="w-10 h-10 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5">
              <Activity size={22} strokeWidth={2.5} />
            </div>
            <div className="flex flex-col justify-center">
              <h1 className="text-sm font-black text-white tracking-[0.25em] leading-none mb-1.5">WATCHDOG</h1>
              <div className="px-2 py-0.5 rounded-full text-[7px] font-black uppercase tracking-[0.1em] w-fit leading-none border text-emerald-500 border-emerald-500/30 bg-emerald-500/5">
                DASHBOARD
              </div>
            </div>
          </div>

          <div className="h-6 w-px bg-white/5 hidden sm:block mx-2"></div>

          <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full border shadow-lg shadow-emerald-500/5 ${apiError ? 'bg-rose-500/10 border-rose-500/20' : 'bg-emerald-500/10 border-emerald-500/20'}`}>
            <div className={`w-2 h-2 rounded-full ${apiError ? 'bg-rose-500' : 'bg-emerald-500 animate-pulse'}`}></div>
            <span className={`text-[10px] font-black uppercase tracking-[0.1em] ${apiError ? 'text-rose-500' : 'text-emerald-500'}`}>
              {apiError ? 'WATCHDOG OFFLINE' : 'WATCHDOG ONLINE'}
            </span>
          </div>
        </div>
        
        <div className="flex items-center gap-8">
          {/* Kullanıcı Bilgisi */}
          <div className="flex items-center gap-3 pr-2">
            <div className="flex flex-col items-end">
              <span className="text-xs font-black text-white leading-none uppercase tracking-wider">{user?.username || 'Berke Buyukkopru'}</span>
              <span className="text-[9px] font-bold text-slate-500 lowercase tracking-tight mt-1">{user?.email || 'admin@watchdog.com'}</span>
            </div>
            <div className="w-10 h-10 rounded-xl bg-white/5 flex items-center justify-center text-slate-400 border border-white/10 shadow-lg">
              <Activity size={18} strokeWidth={2.5} />
            </div>
          </div>

          <button onClick={logout} className="flex items-center gap-2 text-[11px] font-black uppercase tracking-[0.15em] text-rose-500 hover:text-rose-400 transition-all active:scale-95">
            <LogOut size={16} strokeWidth={3} />
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
