import React, { useState } from 'react';
import { Plus, UserCog, History, Users } from 'lucide-react';
import { useAdminManagement } from './hooks/useAdminManagement';
import { AdminTable } from './components/AdminTable';
import { AdminModal } from './components/AdminModal';
import { AuthorizedAppsModal } from './components/AuthorizedAppsModal';
import ConfirmModal from '../../components/common/ConfirmModal';
import type { AdminUser } from '../../types/admin.types';

export const AdminListView: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'active' | 'deleted'>('active');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAppsModalOpen, setIsAppsModalOpen] = useState(false);
  const [selectedAdmin, setSelectedAdmin] = useState<AdminUser | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; adminId: string | null }>({
    isOpen: false,
    adminId: null
  });

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
  
  const handleDeleteClick = (id: string) => {
    setDeleteConfirm({ isOpen: true, adminId: id });
  };

  const handleConfirmDelete = async () => {
    if (deleteConfirm.adminId) {
      await deleteAdmin(deleteConfirm.adminId);
      setDeleteConfirm({ isOpen: false, adminId: null });
    }
  };

  return (
    <div className="p-4 sm:p-8 max-w-7xl mx-auto space-y-8 animate-in fade-in duration-500">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between border-b border-white/5 pb-6 gap-4 sm:gap-0">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5 shrink-0">
            <Users size={24} />
          </div>
          <div>
            <h1 className="text-xl font-black text-white uppercase tracking-[0.2em] leading-tight">Admin Yönetimi</h1>
            <p className="text-xs text-slate-500 font-medium mt-1">Sisteme erişimi olan yöneticileri ve yetkilerini yapılandırın.</p>
          </div>
        </div>
        <button
          onClick={handleAdd}
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
          <UserCog size={14} />
          Aktif Adminler
        </button>
        <button
          onClick={() => setActiveTab('deleted')}
          className={`px-5 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex items-center justify-center gap-2 flex-1 sm:flex-none ${
            activeTab === 'deleted' 
            ? 'bg-rose-600 text-white shadow-lg shadow-rose-600/20' 
            : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <History size={14} />
          Silinen Adminler
        </button>
      </div>

      <AdminTable 
        admins={admins}
        loading={loading}
        activeTab={activeTab}
        onEdit={handleEdit}
        onDelete={handleDeleteClick}
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

      <ConfirmModal
        isOpen={deleteConfirm.isOpen}
        onCancel={() => setDeleteConfirm({ isOpen: false, adminId: null })}
        onConfirm={handleConfirmDelete}
        title="Yöneticiyi Sil"
        message="Bu yöneticiyi silmek istediğinize emin misiniz? Bu işlem sonucunda yönetici pasife çekilecektir."
        confirmText="Evet, Sil"
        cancelText="Vazgeç"
        variant="danger"
      />
    </div>
  );
};
