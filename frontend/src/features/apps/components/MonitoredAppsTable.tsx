import React from 'react';
import { 
  Pencil, 
  Trash2, 
  Globe, 
  Clock, 
  RotateCcw,
  Info 
} from 'lucide-react';

export interface AppListItem {
  id: string;
  name: string;
  url: string;
  interval: number;
  isActive: boolean;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
  deletedAt?: string;
  deletedBy?: string;
}

interface MonitoredAppsTableProps {
  apps: AppListItem[];
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onToggleStatus: (id: string) => void;
  onRestore: (id: string) => void;
  isDeletedMode?: boolean;
}

const MonitoredAppsTable: React.FC<MonitoredAppsTableProps> = ({
  apps,
  onEdit,
  onDelete,
  onToggleStatus,
  onRestore,
  isDeletedMode = false
}) => {
  return (
    <div className="bg-white/5 border border-white/10 rounded-2xl overflow-hidden shadow-xl">

      {/* Table Content */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-slate-800/50 bg-slate-900/40">
              <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest">Uygulama Adı</th>
              <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest">URL</th>
              <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest text-center">Tarama</th>
              <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest text-center">Durum</th>
              <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest text-right">İşlemler</th>
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
                    <div className="flex items-center gap-1.5">
                      <span className="text-sm font-bold text-slate-200">{app.name}</span>
                      <div 
                        title={`Oluşturan: ${app.createdBy || 'Sistem'} (${new Date(app.createdAt).toLocaleDateString('tr-TR')})${app.modifiedAt ? `\nGüncelleyen: ${app.modifiedBy || 'Sistem'} (${new Date(app.modifiedAt).toLocaleDateString('tr-TR')})` : ''}${app.deletedAt ? `\nSilen: ${app.deletedBy || 'Sistem'} (${new Date(app.deletedAt).toLocaleDateString('tr-TR')})` : ''}`}
                        className="cursor-help opacity-50 hover:opacity-100 transition-opacity"
                      >
                        <Info size={14} className="text-slate-500" />
                      </div>
                    </div>
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
                        className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-indigo-400 transition-all opacity-70 hover:opacity-100"
                        title="Geri Yükle"
                      >
                        <RotateCcw size={16} />
                      </button>
                    ) : (
                      <>
                        <button
                          onClick={() => onEdit(app.id)}
                          className="p-2 text-slate-400 hover:text-emerald-400 hover:bg-emerald-500/10 rounded-lg transition-all opacity-70 hover:opacity-100"
                          title="Düzenle"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => onDelete(app.id)}
                          className="p-2 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-all opacity-70 hover:opacity-100"
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
