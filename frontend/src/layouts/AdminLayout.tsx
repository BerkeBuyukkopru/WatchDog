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
      <header className="h-16 bg-background border-b border-white/5 flex items-center justify-between px-4 sm:px-6 shrink-0">
        <div className="flex items-center gap-3 sm:gap-6">
          <div className="flex items-center gap-3 sm:gap-4">
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5">
              <Activity className="w-5 h-5 sm:w-[22px] sm:h-[22px]" strokeWidth={2.5} />
            </div>
            <div className="flex flex-col justify-center">
              <h1 className="text-[10px] sm:text-sm font-black text-white tracking-[0.2em] sm:tracking-[0.25em] leading-none mb-1 sm:mb-1.5">WATCHDOG</h1>
              <div className="px-1.5 sm:px-2 py-0.5 rounded-full text-[6px] sm:text-[7px] font-black uppercase tracking-[0.1em] w-fit leading-none border text-emerald-500 border-emerald-500/30 bg-emerald-500/5">
                DASHBOARD
              </div>
            </div>
          </div>

          <div className="h-6 w-px bg-white/5 hidden md:block mx-2"></div>

          <div className={`flex items-center gap-1.5 sm:gap-2 px-2 sm:px-3 py-1 sm:py-1.5 rounded-full border shadow-lg shadow-emerald-500/5 ${apiError ? 'bg-rose-500/10 border-rose-500/20' : 'bg-emerald-500/10 border-emerald-500/20'}`}>
            <div className={`w-1.5 h-1.5 sm:w-2 sm:h-2 rounded-full ${apiError ? 'bg-rose-500' : 'bg-emerald-500 animate-pulse'}`}></div>
            <span className={`text-[8px] sm:text-[10px] font-black uppercase tracking-[0.1em] ${apiError ? 'text-rose-500' : 'text-emerald-500'}`}>
              {apiError ? 'OFFLINE' : 'ONLINE'}
            </span>
          </div>
        </div>
        
        <div className="flex items-center gap-4 sm:gap-8">
          {/* Kullanıcı Bilgisi - Mobilde gizle */}
          <div className="hidden sm:flex items-center gap-3 pr-2">
            <div className="flex flex-col items-end">
              <span className="text-xs font-black text-white leading-none uppercase tracking-wider">{user?.username || 'Berke Buyukkopru'}</span>
              <span className="text-[9px] font-bold text-slate-500 lowercase tracking-tight mt-1">{user?.email || 'admin@watchdog.com'}</span>
            </div>
            <div className="w-10 h-10 rounded-xl bg-white/5 flex items-center justify-center text-slate-400 border border-white/10 shadow-lg">
              <Activity size={18} strokeWidth={2.5} />
            </div>
          </div>

          <button onClick={logout} className="flex items-center gap-2 text-[10px] sm:text-[11px] font-black uppercase tracking-[0.15em] text-rose-500 hover:text-rose-400 transition-all active:scale-95">
            <LogOut className="w-3.5 h-3.5 sm:w-4 sm:h-4" strokeWidth={3} />
            <span className="hidden xs:inline">Çıkış</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-auto custom-scrollbar p-4 sm:p-6 lg:p-8">
        <Outlet context={{ setApiError }} />
      </main>
    </div>
  );
};

export default AdminLayout;
