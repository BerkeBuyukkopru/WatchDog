import React from 'react';
import { 
  Pencil, 
  Trash2, 
  Plus, 
  Globe, 
  Clock, 
  RotateCcw 
} from 'lucide-react';

export interface AppListItem {
  id: string;
  name: string;
  url: string;
  interval: number;
  isActive: boolean;
}

interface MonitoredAppsTableProps {
  apps: AppListItem[];
  onAddClick: () => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onToggleStatus: (id: string) => void;
  onRestore: (id: string) => void;
  isDeletedMode?: boolean;
}

const MonitoredAppsTable: React.FC<MonitoredAppsTableProps> = ({
  apps,
  onAddClick,
  onEdit,
  onDelete,
  onToggleStatus,
  onRestore,
  isDeletedMode = false
}) => {
  return (
    <div className="bg-background-light border border-slate-800 rounded-2xl overflow-hidden shadow-2xl">
      {/* Table Header Area */}
      <div className="h-[60px] px-6 border-b border-slate-800 flex items-center justify-between bg-slate-800/10">
        <div className="flex items-center gap-3">
          {isDeletedMode ? (
            <Trash2 size={18} className="text-rose-400" />
          ) : (
            <Globe size={18} className="text-indigo-400" />
          )}
          <h2 className="text-sm font-black text-slate-100 uppercase tracking-[0.2em]">
            {isDeletedMode ? 'Silinmiş Uygulamalar' : 'İzlenen Uygulamalar'}
          </h2>
        </div>
        {!isDeletedMode && (
          <button
            onClick={onAddClick}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-[11px] font-black uppercase tracking-widest rounded-xl transition-all shadow-[0_0_15px_rgba(79,70,229,0.2)] active:scale-95"
          >
            <Plus size={14} />
            Yeni Ekle
          </button>
        )}
      </div>

      {/* Table Content */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-slate-800/50 bg-slate-900/40">
              <th className="px-6 py-4 text-[10px] font-black text-slate-500 uppercase tracking-widest">Uygulama Adı</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-500 uppercase tracking-widest">URL</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-500 uppercase tracking-widest text-center">Tarama</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-500 uppercase tracking-widest text-center">Durum</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-500 uppercase tracking-widest text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/30">
            {apps.map((app) => (
              <tr key={app.id} className="hover:bg-slate-800/20 transition-colors group">
                <td className="px-6 py-4">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-slate-800 flex items-center justify-center text-slate-400 group-hover:text-indigo-400 transition-colors">
                      <Globe size={14} />
                    </div>
                    <span className="text-sm font-bold text-slate-200">{app.name}</span>
                  </div>
                </td>
                <td className="px-6 py-4">
                  <span className="text-xs text-slate-500 font-medium">{app.url}</span>
                </td>
                <td className="px-6 py-4 text-center">
                  <div className="inline-flex items-center gap-1.5 px-2 py-1 bg-slate-800/50 rounded-md text-slate-400">
                    <Clock size={12} />
                    <span className="text-xs font-bold">{app.interval}s</span>
                  </div>
                </td>
                <td className="px-6 py-4">
                  <div className="flex justify-center">
                    <button
                      onClick={() => !isDeletedMode && onToggleStatus(app.id)}
                      disabled={isDeletedMode}
                      className={`relative inline-flex h-5 w-10 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none ${
                        isDeletedMode ? 'bg-slate-800 opacity-50 cursor-not-allowed' :
                        app.isActive ? 'bg-emerald-500' : 'bg-slate-700'
                      }`}
                    >
                      <span
                        className={`pointer-events-none inline-block h-4 w-4 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                          app.isActive ? 'translate-x-5' : 'translate-x-0'
                        }`}
                      />
                    </button>
                  </div>
                </td>
                <td className="px-6 py-4 text-right">
                  <div className="flex items-center justify-end gap-2">
                    {isDeletedMode ? (
                      <button
                        onClick={() => onRestore(app.id)}
                        className="flex items-center gap-2 px-3 py-1.5 bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500 hover:text-white rounded-lg transition-all text-[10px] font-black uppercase tracking-widest"
                        title="Geri Yükle"
                      >
                        <RotateCcw size={14} />
                        Geri Yükle
                      </button>
                    ) : (
                      <>
                        <button
                          onClick={() => onEdit(app.id)}
                          className="p-2 text-slate-400 hover:text-amber-400 hover:bg-amber-500/10 rounded-lg transition-all"
                          title="Düzenle"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => onDelete(app.id)}
                          className="p-2 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-all"
                          title="Sil"
                        >
                          <Trash2 size={16} />
                        </button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MonitoredAppsTable;
