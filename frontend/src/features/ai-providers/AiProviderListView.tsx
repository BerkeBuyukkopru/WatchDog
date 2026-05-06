import React, { useState } from 'react';
import { Plus, BrainCircuit, RotateCcw } from 'lucide-react';
import { AiProviderTable } from './components/AiProviderTable';
import { AiProviderModal } from './components/AiProviderModal';
import { useAiProviders } from './hooks/useAiProviders';
import type { AiProviderDetail } from '../../types/ai-provider.types';

export const AiProviderListView: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'active' | 'deleted'>('active');
  const { providers, loading, toggleStatus, deleteProvider, restoreProvider, refresh } = useAiProviders(activeTab);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<AiProviderDetail | null>(null);

  const handleEdit = (provider: AiProviderDetail) => {
    setSelectedProvider(provider);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedProvider(null);
    setIsModalOpen(true);
  };

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8 animate-in fade-in duration-500">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-black text-white tracking-tight flex items-center gap-3">
            AI Sağlayıcıları
          </h1>
          <p className="text-slate-500 mt-2 font-medium">Sistemin log analizi ve durum raporlaması için kullandığı yapay zeka modelleri.</p>
        </div>
        <button 
          onClick={handleAddNew}
          className="bg-indigo-600 hover:bg-indigo-500 text-white px-6 py-3 rounded-2xl font-bold transition-all shadow-lg shadow-indigo-600/20 flex items-center gap-2 active:scale-95"
        >
          <Plus size={20} />
          Yeni Ekle
        </button>
      </div>

      <div className="flex gap-2 p-1 bg-white/5 rounded-2xl w-fit border border-white/5">
        <button
          onClick={() => setActiveTab('active')}
          className={`flex items-center gap-2 px-6 py-2.5 rounded-xl text-sm font-bold transition-all ${
            activeTab === 'active' 
              ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' 
              : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <BrainCircuit size={16} />
          Aktif Sağlayıcılar
        </button>
        <button
          onClick={() => setActiveTab('deleted')}
          className={`flex items-center gap-2 px-6 py-2.5 rounded-xl text-sm font-bold transition-all ${
            activeTab === 'deleted' 
              ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' 
              : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <RotateCcw size={16} />
          Dondurulanlar
        </button>
      </div>

      <AiProviderTable 
        providers={providers}
        loading={loading}
        activeTab={activeTab}
        onEdit={handleEdit}
        onDelete={deleteProvider}
        onRestore={restoreProvider}
        onToggle={toggleStatus}
      />

      <AiProviderModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={refresh}
        provider={selectedProvider}
      />
    </div>
  );
};
