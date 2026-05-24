import React, { useState, useEffect } from 'react';
import MonitoredAppsTable, { type AppListItem } from './components/MonitoredAppsTable';
import AppFormModal, { type AppFormData } from './components/AppFormModal';
import { appsService } from '../../api/appsService';
import { getApiErrorMessage } from '../../api/apiError';
import { LayoutGrid, Loader2, Trash2, CheckCircle2, Plus } from 'lucide-react';
import { toast } from 'sonner';
import ConfirmModal from '../../components/common/ConfirmModal';

const AppsManagementPage: React.FC = () => {
  const [apps, setApps] = useState<AppListItem[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  
  // Modal States
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [editingApp, setEditingApp] = useState<AppFormData | undefined>(undefined);
  const [editingAppId, setEditingAppId] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<'active' | 'deleted'>('active');
  
  // Confirm Modal States
  const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean; id: string | null }>({
    isOpen: false,
    id: null
  });

  useEffect(() => {
    fetchApps();
  }, [viewMode]);



  const fetchApps = async () => {
    try {
      setIsLoading(true);
      const data = viewMode === 'active' 
        ? await appsService.getAllApps()
        : await appsService.getDeletedApps();
        
      // AppDto'yu AppListItem'a çevir
      const mappedApps: AppListItem[] = data.map(app => ({
        id: app.id,
        name: app.name,
        url: app.healthUrl,
        interval: app.pollingIntervalSeconds,
        isActive: app.isActive,
        createdAt: app.createdAt,
        createdBy: app.createdBy,
        modifiedAt: app.modifiedAt,
        modifiedBy: app.modifiedBy,
        deletedAt: app.deletedAt,
        deletedBy: app.deletedBy
      }));
      setApps(mappedApps);
    } catch (error) {
      console.error('Uygulamalar yüklenirken hata oluştu:', error);
      toast.error('Uygulamalar yüklenemedi.');
    } finally {
      setIsLoading(false);
    }
  };

  // --- Handlers ---

  const handleAddClick = () => {
    setEditingApp(undefined);
    setEditingAppId(null);
    setIsModalOpen(true);
  };

  const handleEditClick = (id: string) => {
    const appToEdit = apps.find(a => a.id === id);
    if (appToEdit) {
      setEditingApp({
        name: appToEdit.name,
        healthUrl: appToEdit.url,
        pollingIntervalSeconds: appToEdit.interval,
        isActive: appToEdit.isActive
      });
      setEditingAppId(id);
      setIsModalOpen(true);
    }
  };

  const handleDeleteClick = (id: string) => {
    setConfirmDelete({ isOpen: true, id });
  };

  const handleConfirmDelete = async () => {
    if (!confirmDelete.id) return;
    
    try {
      await appsService.deleteApp(confirmDelete.id);
      setApps(prev => prev.filter(a => a.id !== confirmDelete.id));
      toast.success('Uygulama silindi (Çöp kutusuna taşındı).');
    } catch (error) {
      toast.error(getApiErrorMessage(error, 'Silme işlemi başarısız oldu.'));
    } finally {
      setConfirmDelete({ isOpen: false, id: null });
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await appsService.restoreApp(id);
      setApps(prev => prev.filter(a => a.id !== id));
      toast.success('Uygulama başarıyla geri yüklendi.');
    } catch (error) {
      toast.error(getApiErrorMessage(error, 'Geri yükleme işlemi başarısız oldu.'));
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      // Optimistic update
      setApps(prev => prev.map(a => a.id === id ? { ...a, isActive: !a.isActive } : a));
      await appsService.toggleAppStatus(id);
    } catch (error) {
      console.error('Durum değiştirme başarısız:', error);
      // Rollback
      setApps(prev => prev.map(a => a.id === id ? { ...a, isActive: !a.isActive } : a));
    }
  };



  const handleModalSubmit = async (formData: AppFormData) => {
    try {
      if (editingAppId) {
        // Edit Mode
        await appsService.updateApp(editingAppId, {
          id: editingAppId,
          ...formData
        });
        toast.success('Uygulama başarıyla güncellendi.');
      } else {
        // Create Mode
        await appsService.createApp({
          ...formData
        });
        toast.success('Yeni uygulama başarıyla eklendi.');
      }
      setIsModalOpen(false);
      fetchApps(); // Listeyi yenile
    } catch (error) {
      toast.error(getApiErrorMessage(error, 'Kayıt işlemi sırasında bir hata oluştu.'));
    }
  };

  return (
    <div className="p-4 sm:p-8 max-w-7xl mx-auto space-y-8 animate-in fade-in duration-500">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between border-b border-white/5 pb-6 gap-4 sm:gap-0">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5 shrink-0">
            <LayoutGrid size={24} />
          </div>
          <div>
            <h1 className="text-xl font-black text-white uppercase tracking-[0.2em] leading-tight">Uygulama Yönetimi</h1>
            <p className="text-xs text-slate-500 font-medium mt-1">Sistemdeki tüm uygulamaları izleyin, ekleyin ve yapılandırın.</p>
          </div>
        </div>
        {!isLoading && viewMode === 'active' && (
          <button
            onClick={handleAddClick}
            className="bg-indigo-600 hover:bg-indigo-500 text-white px-5 py-2.5 rounded-xl flex items-center justify-center gap-2 transition-all font-bold text-xs uppercase tracking-widest shadow-lg shadow-indigo-600/20 active:scale-95 w-full sm:w-auto"
          >
            <Plus size={16} strokeWidth={3} />
            Yeni Ekle
          </button>
        )}
      </div>

      {/* Tabs Switcher */}
      <div className="flex flex-col sm:flex-row gap-1.5 p-1.5 bg-white/[0.03] w-full sm:w-fit rounded-2xl border border-white/5 shadow-inner">
        <button
          onClick={() => setViewMode('active')}
          className={`flex items-center justify-center gap-2 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex-1 sm:flex-none ${
            viewMode === 'active' 
            ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' 
            : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <CheckCircle2 size={14} />
          İzlenen Uygulamalar
        </button>
        <button
          onClick={() => setViewMode('deleted')}
          className={`flex items-center justify-center gap-2 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex-1 sm:flex-none ${
            viewMode === 'deleted' 
            ? 'bg-rose-600 text-white shadow-lg shadow-rose-600/20' 
            : 'text-slate-500 hover:text-slate-300 hover:bg-white/5'
          }`}
        >
          <Trash2 size={14} />
          Silinen Uygulamalar
        </button>
      </div>

      {/* Content */}
      <div className="w-full">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-20 text-slate-500 gap-3">
            <Loader2 size={32} className="animate-spin text-indigo-500" />
            <span className="text-sm font-semibold animate-pulse">Uygulamalar yükleniyor...</span>
          </div>
        ) : (
          <MonitoredAppsTable 
            apps={apps}
            onEdit={handleEditClick}
            onDelete={handleDeleteClick}
            onToggleStatus={handleToggleStatus}
            onRestore={handleRestore}
            isDeletedMode={viewMode === 'deleted'}
          />
        )}
      </div>

      {/* Add/Edit Modal */}
      <AppFormModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleModalSubmit}
        initialData={editingApp}
        isEdit={Boolean(editingAppId)}
      />

      <ConfirmModal
        isOpen={confirmDelete.isOpen}
        title="Uygulamayı Sil"
        message="Bu uygulamayı silmek istediğinize emin misiniz? Uygulama 'Silinenler' listesine taşınacaktır."
        confirmText="Evet, Sil"
        cancelText="Vazgeç"
        onConfirm={handleConfirmDelete}
        onCancel={() => setConfirmDelete({ isOpen: false, id: null })}
      />
    </div>
  );
};

export default AppsManagementPage;
