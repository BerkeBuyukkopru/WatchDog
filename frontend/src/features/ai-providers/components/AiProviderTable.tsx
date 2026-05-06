import React, { useState } from 'react';
import { BrainCircuit, Edit2, Trash2, Globe, Lock, Eye, EyeOff, RotateCcw } from 'lucide-react';
import type { AiProviderDetail } from '../../../types/ai-provider.types';

interface AiProviderTableProps {
  providers: AiProviderDetail[];
  loading: boolean;
  activeTab: 'active' | 'deleted';
  onEdit: (provider: AiProviderDetail) => void;
  onDelete: (id: string) => void;
  onRestore: (id: string) => void;
  onToggle: (id: string) => void;
}

export const AiProviderTable: React.FC<AiProviderTableProps> = ({
  providers,
  loading,
  activeTab,
  onEdit,
  onDelete,
  onRestore,
  onToggle
}) => {
  const [showKeys, setShowKeys] = useState<Record<string, boolean>>({});

  const toggleKey = (id: string) => {
    setShowKeys(prev => ({ ...prev, [id]: !prev[id] }));
  };

  return (
    <div className="bg-white/5 border border-white/10 rounded-2xl overflow-hidden shadow-xl">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-white/5 text-[11px] uppercase tracking-wider text-slate-500 font-black">
              <th className="px-6 py-4">Sağlayıcı Adı</th>
              <th className="px-6 py-4">Model</th>
              <th className="px-6 py-4">API Endpoint</th>
              <th className="px-6 py-4">API Key</th>
              <th className="px-6 py-4">Durum</th>
              <th className="px-6 py-4 text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {loading ? (
              <tr>
                <td colSpan={6} className="px-6 py-12 text-center text-slate-500">
                  <div className="flex flex-col items-center gap-3">
                    <div className="w-6 h-6 border-2 border-indigo-500/20 border-t-indigo-500 rounded-full animate-spin"></div>
                    Yükleniyor...
                  </div>
                </td>
              </tr>
            ) : providers.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-12 text-center text-slate-500 italic">
                  Kayıtlı AI sağlayıcısı bulunamadı.
                </td>
              </tr>
            ) : (
              providers.map((provider) => (
                <tr key={provider.id} className="hover:bg-white/[0.02] transition-colors group">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-400">
                        <BrainCircuit size={16} />
                      </div>
                      <span className="text-sm font-semibold text-slate-200">{provider.name}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className="text-xs font-mono text-indigo-300 bg-indigo-500/10 px-2 py-0.5 rounded border border-indigo-500/20">
                      {provider.modelName}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2 text-sm text-slate-500">
                      <Globe size={14} />
                      <span className="truncate max-w-[150px]">{provider.apiUrl}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className="flex items-center gap-2 px-2 py-1 bg-black/20 rounded-lg border border-white/5 min-w-[120px]">
                        <Lock size={12} className="text-slate-500" />
                        <span className="text-[11px] font-mono text-slate-400 break-all">
                          {showKeys[provider.id] ? provider.apiKey : '••••••••••••'}
                        </span>
                      </div>
                      <button 
                        onClick={() => toggleKey(provider.id)}
                        className="p-1 hover:bg-white/10 rounded-md text-slate-500 hover:text-indigo-400 transition-all"
                      >
                        {showKeys[provider.id] ? <Eye size={14} /> : <EyeOff size={14} />}
                      </button>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <button
                      onClick={() => onToggle(provider.id)}
                      disabled={activeTab === 'deleted'}
                      className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none ${
                        provider.isActive ? 'bg-emerald-500' : 'bg-slate-700'
                      } ${activeTab === 'deleted' ? 'opacity-50 cursor-not-allowed' : ''}`}
                    >
                      <span
                        className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                          provider.isActive ? 'translate-x-6' : 'translate-x-1'
                        }`}
                      />
                    </button>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                      {activeTab === 'active' ? (
                        <>
                          <button 
                            onClick={() => onEdit(provider)}
                            className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-emerald-400 transition-all" 
                            title="Düzenle"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => onDelete(provider.id)}
                            className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-rose-400 transition-all" 
                            title="Sil"
                          >
                            <Trash2 size={16} />
                          </button>
                        </>
                      ) : (
                        <button 
                          onClick={() => onRestore(provider.id)}
                          className="p-2 hover:bg-white/10 rounded-lg text-slate-400 hover:text-indigo-400 transition-all" 
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
