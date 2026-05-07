import React from 'react';
import { Edit2, Trash2, RotateCcw, Key, Info } from 'lucide-react';
import type { AdminUser } from '../../../types/admin.types';

interface AdminTableProps {
  admins: AdminUser[];
  loading: boolean;
  activeTab: 'active' | 'deleted';
  onEdit: (admin: AdminUser) => void;
  onDelete: (id: string) => void;
  onRestore: (id: string) => void;
  onShowApps: (admin: AdminUser) => void;
}

export const AdminTable: React.FC<AdminTableProps> = ({ 
  admins, 
  loading, 
  activeTab, 
  onEdit, 
  onDelete, 
  onRestore,
  onShowApps
}) => {
  return (
    <div className="bg-white/5 border border-white/10 rounded-2xl overflow-hidden shadow-xl">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-slate-800/50 bg-slate-900/40 text-[11px] uppercase tracking-widest text-slate-500 font-black">
              <th className="px-6 py-4">İsim</th>
              <th className="px-6 py-4">Email</th>
              <th className="px-6 py-4">Rol</th>
              <th className="px-6 py-4 text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/30">
            {loading ? (
              <tr>
                <td colSpan={4} className="px-6 py-12 text-center text-slate-500">
                  <div className="flex flex-col items-center gap-3">
                    <div className="w-6 h-6 border-2 border-indigo-500/20 border-t-indigo-500 rounded-full animate-spin"></div>
                    Yükleniyor...
                  </div>
                </td>
              </tr>
            ) : admins.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-6 py-12 text-center text-slate-500 italic">
                  Gösterilecek yönetici bulunamadı.
                </td>
              </tr>
            ) : (
              admins.map((admin) => (
                <tr key={admin.id} className="hover:bg-white/[0.02] transition-colors group">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-indigo-500/20 flex items-center justify-center text-indigo-400 font-bold text-xs">
                        {admin.username.substring(0, 2).toUpperCase()}
                      </div>
                      <div className="flex flex-col">
                        <div className="flex items-center gap-1.5">
                          <span className="text-sm font-semibold text-slate-200">{admin.username}</span>
                          <div 
                            title={`Oluşturan: ${admin.createdBy || 'Sistem'} (${new Date(admin.createdAt).toLocaleDateString('tr-TR')})${admin.modifiedAt ? `\nGüncelleyen: ${admin.modifiedBy || 'Sistem'} (${new Date(admin.modifiedAt).toLocaleDateString('tr-TR')})` : ''}${admin.deletedAt ? `\nSilen: ${admin.deletedBy || 'Sistem'} (${new Date(admin.deletedAt).toLocaleDateString('tr-TR')})` : ''}`}
                            className="cursor-help opacity-50 hover:opacity-100 transition-opacity"
                          >
                            <Info size={14} className="text-slate-500" />
                          </div>
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-slate-400">{admin.email}</td>
                  <td className="px-6 py-4">
                    <span className={`px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-wider ${
                      admin.role === 'SuperAdmin' 
                      ? 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/20' 
                      : 'bg-slate-700/30 text-slate-400 border border-slate-700/50'
                    }`}>
                      {admin.role}
                    </span>
                  </td>

                  <td className="px-6 py-4 text-right">
                    <div className="flex justify-end gap-2">
                      {activeTab === 'active' ? (
                        <>
                          <button 
                            onClick={() => onShowApps(admin)}
                            className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-indigo-400 transition-all opacity-70 hover:opacity-100" 
                            title="Yetkili Uygulamalar"
                          >
                            <Key size={16} />
                          </button>
                          <button 
                            onClick={() => onEdit(admin)}
                            className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-emerald-400 transition-all opacity-70 hover:opacity-100" 
                            title="Düzenle"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => onDelete(admin.id)}
                            className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-rose-400 transition-all opacity-70 hover:opacity-100" 
                            title="Sil"
                          >
                            <Trash2 size={16} />
                          </button>
                        </>
                      ) : (
                        <button 
                          onClick={() => onRestore(admin.id)}
                          className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-indigo-400 transition-all opacity-70 hover:opacity-100" 
                          title="Geri Yükle"
                        >
                          <RotateCcw size={16} />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
