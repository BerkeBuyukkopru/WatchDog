import React, { useState } from 'react';
import { Plus, UserCog, History } from 'lucide-react';
import { useAdminManagement } from './hooks/useAdminManagement';
import { AdminTable } from './components/AdminTable';
import { AdminModal } from './components/AdminModal';
import { AuthorizedAppsModal } from './components/AuthorizedAppsModal';
import type { AdminUser } from '../../types/admin.types';

export const AdminListView: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'active' | 'deleted'>('active');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAppsModalOpen, setIsAppsModalOpen] = useState(false);
  const [selectedAdmin, setSelectedAdmin] = useState<AdminUser | null>(null);

  const { admins, loading, availableApps, refresh, deleteAdmin, restoreAdmin } = useAdminManagement(activeTab);

  const handleEdit = (admin: AdminUser) => {
    setSelectedAdmin(admin);
    setIsModalOpen(true);
  };

  const handleAdd = () => {
    setSelectedAdmin(null);
    setIsModalOpen(true);
  };

  const handleShowApps = (admin: AdminUser) => {
    setSelectedAdmin(admin);
    setIsAppsModalOpen(true);
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-white">Admin Yönetimi</h1>
          <p className="text-slate-400 text-sm mt-1">Sisteme erişimi olan yöneticileri ve yetkilerini yapılandırın.</p>
        </div>
        <button
          onClick={handleAdd}
          className="bg-indigo-600 hover:bg-indigo-500 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-all font-medium shadow-lg shadow-indigo-600/20"
        >
          <Plus size={18} />
          Yeni Ekle
        </button>
      </div>

      <div className="flex gap-2 p-1 bg-white/5 w-fit rounded-xl border border-white/10">
        <button
          onClick={() => setActiveTab('active')}
          className={`px-4 py-2 rounded-lg text-sm font-bold transition-all flex items-center gap-2 ${
            activeTab === 'active' ? 'bg-indigo-600 text-white' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <UserCog size={16} />
          Aktif Adminler
        </button>
        <button
          onClick={() => setActiveTab('deleted')}
          className={`px-4 py-2 rounded-lg text-sm font-bold transition-all flex items-center gap-2 ${
            activeTab === 'deleted' ? 'bg-indigo-600 text-white' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <History size={16} />
          Silinen Adminler
        </button>
      </div>

      <AdminTable 
        admins={admins}
        loading={loading}
        activeTab={activeTab}
        onEdit={handleEdit}
        onDelete={deleteAdmin}
        onRestore={restoreAdmin}
        onShowApps={handleShowApps}
      />

      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={refresh}
        admin={selectedAdmin}
        availableApps={availableApps}
      />

      <AuthorizedAppsModal
        isOpen={isAppsModalOpen}
        onClose={() => setIsAppsModalOpen(false)}
        admin={selectedAdmin}
        availableApps={availableApps}
      />
    </div>
  );
};
