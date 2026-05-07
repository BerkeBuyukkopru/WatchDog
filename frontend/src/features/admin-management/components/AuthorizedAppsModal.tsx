import React from 'react';
import { X, ShieldCheck, Activity } from 'lucide-react';
import type { AdminUser, MonitoredApp } from '../../../types/admin.types';

interface AuthorizedAppsModalProps {
  isOpen: boolean;
  onClose: () => void;
  admin: AdminUser | null;
  availableApps: MonitoredApp[];
}

export const AuthorizedAppsModal: React.FC<AuthorizedAppsModalProps> = ({ isOpen, onClose, admin, availableApps }) => {
  if (!isOpen || !admin) return null;

  // Adminin ID listesini kullanarak tüm uygulamalar arasından yetkili olduklarını buluyoruz
  const authorizedApps = availableApps.filter(app => 
    admin.allowedAppIds?.includes(app.id)
  );

  return (
    <div className="fixed top-0 left-0 w-full h-full z-[9999] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/80 backdrop-blur-sm animate-in fade-in duration-300" 
        onClick={onClose}
      />

      {/* Modal Box */}
      <div className="relative z-10 bg-[#16161A] border border-white/10 rounded-2xl w-full max-w-md shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200 max-h-[90vh] flex flex-col">
        <div className="p-5 border-b border-white/5 flex items-center justify-between bg-white/5">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-500/20 rounded-lg text-indigo-400">
              <ShieldCheck size={20} />
            </div>
            <div>
              <h2 className="text-lg font-bold text-white">Yetki Detayları</h2>
              <p className="text-[10px] text-slate-500 font-medium uppercase tracking-wider">{admin.username}</p>
            </div>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-white/10 rounded-full text-slate-400 transition-colors">
            <X size={18} />
          </button>
        </div>

        <div className="p-6">
          <h3 className="text-xs font-black text-slate-500 uppercase tracking-widest mb-4">Yetkili Olduğu Uygulamalar</h3>
          
          <div className="space-y-2 max-h-[300px] overflow-y-auto custom-scrollbar pr-2">
            {authorizedApps.length > 0 ? (
              authorizedApps.map(app => (
                <div 
                  key={app.id}
                  className="flex items-center justify-between p-3 bg-white/5 border border-white/5 rounded-xl hover:bg-white/10 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-400">
                      <Activity size={16} />
                    </div>
                    <span className="text-sm font-semibold text-slate-200">{app.name}</span>
                  </div>
                    <span className={`text-[10px] font-bold px-2 py-1 rounded border ${
                      app.isActive 
                      ? 'text-emerald-500/60 bg-emerald-500/5 border-emerald-500/10' 
                      : 'text-slate-500/60 bg-slate-500/5 border-slate-500/10'
                    }`}>
                      {app.isActive ? 'İZLENİYOR' : 'PASİF'}
                    </span>
                </div>
              ))
            ) : (
              <div className="text-center py-8">
                <p className="text-sm text-slate-500 italic">Henüz yetkilendirilmiş uygulama bulunmuyor.</p>
              </div>
            )}
          </div>
        </div>

        <div className="p-5 border-t border-white/5 bg-white/5 flex justify-end">
          <button
            onClick={onClose}
            className="px-6 py-2 bg-white/5 hover:bg-white/10 text-slate-300 text-sm font-bold rounded-xl transition-all"
          >
            Kapat
          </button>
        </div>
      </div>
    </div>
  );
};
