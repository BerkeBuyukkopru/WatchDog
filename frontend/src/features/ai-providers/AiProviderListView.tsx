import React, { useState } from 'react';
import { Plus, BrainCircuit, Trash2 } from 'lucide-react';
import { AiProviderTable } from './components/AiProviderTable';
import { AiProviderModal } from './components/AiProviderModal';
import { useAiProviders } from './hooks/useAiProviders';
import type { AiProviderDetail } from '../../types/ai-provider.types';
import ConfirmModal from '../../components/common/ConfirmModal';

export const AiProviderListView: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'active' | 'deleted'>('active');
  const { providers, loading, toggleStatus, deleteProvider, restoreProvider, refresh } = useAiProviders(activeTab);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<AiProviderDetail | null>(null);
  
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; providerId: string | null }>({
    isOpen: false,
    providerId: null
  });

  const handleEdit = (provider: AiProviderDetail) => {
    setSelectedProvider(provider);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedProvider(null);
    setIsModalOpen(true);
  };

  const handleDeleteClick = (id: string) => {
    setDeleteConfirm({ isOpen: true, providerId: id });
  };

  const handleConfirmDelete = async () => {
    if (deleteConfirm.providerId) {
      await deleteProvider(deleteConfirm.providerId);
      setDeleteConfirm({ isOpen: false, providerId: null });
    }
  };

  return (
    <div className="p-4 sm:p-8 max-w-7xl mx-auto space-y-8 animate-in fade-in duration-500">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between border-b border-white/5 pb-6 gap-4 sm:gap-0">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5 shrink-0">
            <BrainCircuit size={24} />
          </div>
          <div>
            <h1 className="text-xl font-black text-white uppercase tracking-[0.2em] leading-tight">AI Sağlayıcıları</h1>
            <p className="text-xs text-slate-500 font-medium mt-1">Sistemin log analizi ve durum raporlaması için kullandığı yapay zeka modelleri.</p>
          </div>
        </div>
        <button 
          onClick={handleAddNew}
          className="bg-indigo-600 hover:bg-indigo-500 text-white px-5 py-2.5 rounded-xl flex items-center justify-center gap-2 transition-all font-bold text-xs uppercase tracking-widest shadow-lg shadow-indigo-600/20 active:scale-95 w-full sm:w-auto"
        >
          <Plus size={16} strokeWidth={3} />
          Yeni Ekle
        </button>
      </div>

      <div className="flex flex-col sm:flex-row gap-1.5 p-1.5 bg-white/[0.03] w-full sm:w-fit rounded-2xl border border-white/5 shadow-inner">
        <button
          onClick={() => setActiveTab('active')}
          className={`px-5 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex items-center justify-center gap-2 flex-1 sm:flex-none ${
            activeTab === 'active' 
              ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' 
              : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <BrainCircuit size={14} />
          Aktif Sağlayıcılar
        </button>
        <button
          onClick={() => setActiveTab('deleted')}
          className={`px-5 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex items-center justify-center gap-2 flex-1 sm:flex-none ${
            activeTab === 'deleted' 
              ? 'bg-rose-600 text-white shadow-lg shadow-rose-600/20' 
              : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <Trash2 size={14} />
          Silinen Sağlayıcılar
        </button>
      </div>

      <AiProviderTable 
        providers={providers}
        loading={loading}
        activeTab={activeTab}
        onEdit={handleEdit}
        onDelete={handleDeleteClick}
        onRestore={restoreProvider}
        onToggle={toggleStatus}
      />

      <AiProviderModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={refresh}
        provider={selectedProvider}
      />

      <ConfirmModal
        isOpen={deleteConfirm.isOpen}
        onCancel={() => setDeleteConfirm({ isOpen: false, providerId: null })}
        onConfirm={handleConfirmDelete}
        title="Sağlayıcıyı Sil"
        message="Bu AI sağlayıcısını dondurmak istediğinize emin misiniz? Sağlayıcı 'Silinenler' listesine taşınacaktır."
        confirmText="Evet, Sil"
        cancelText="Vazgeç"
        variant="danger"
      />
    </div>
  );
};
