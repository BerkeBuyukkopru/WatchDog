import React, { useState, useEffect } from 'react';
import MonitoredAppsTable, { type AppListItem } from './components/MonitoredAppsTable';
import AppFormModal, { type AppFormData } from './components/AppFormModal';
import { appsService } from '../../api/appsService';
import { LayoutGrid, Loader2, Trash2, CheckCircle2 } from 'lucide-react';
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
        isActive: app.isActive
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
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Silme işlemi başarısız oldu.';
      toast.error(msg);
    } finally {
      setConfirmDelete({ isOpen: false, id: null });
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await appsService.restoreApp(id);
      setApps(prev => prev.filter(a => a.id !== id));
      toast.success('Uygulama başarıyla geri yüklendi.');
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Geri yükleme işlemi başarısız oldu.';
      toast.error(msg);
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
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Kayıt işlemi sırasında bir hata oluştu.';
      toast.error(msg);
    }
  };

  return (
    <div className="flex flex-col gap-6 p-4 md:p-6 w-full max-w-7xl mx-auto">
      {/* Page Header */}
      <div className="flex items-center gap-4 border-b border-slate-800 pb-4">
        <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-500">
          <LayoutGrid size={24} />
        </div>
        <div>
          <h1 className="text-xl font-black text-slate-100 uppercase tracking-widest">Uygulama Yönetimi</h1>
          <p className="text-xs text-slate-500 font-medium mt-1">Sistemdeki tüm uygulamaları izleyin, ekleyin ve yapılandırın.</p>
        </div>
      </div>

      {/* Tabs Switcher */}
      <div className="flex items-center gap-1 bg-slate-900/50 p-1 rounded-xl w-fit border border-slate-800">
        <button
          onClick={() => setViewMode('active')}
          className={`flex items-center gap-2 px-6 py-2 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all ${
            viewMode === 'active' 
            ? 'bg-indigo-600 text-white shadow-lg' 
            : 'text-slate-500 hover:text-slate-300'
          }`}
        >
          <CheckCircle2 size={14} />
          İzlenenler
        </button>
        <button
          onClick={() => setViewMode('deleted')}
          className={`flex items-center gap-2 px-6 py-2 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all ${
            viewMode === 'deleted' 
            ? 'bg-rose-600 text-white shadow-lg' 
            : 'text-slate-500 hover:text-slate-300'
          }`}
        >
          <Trash2 size={14} />
          Silinenler
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
            onAddClick={handleAddClick}
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
        isEdit={!!editingAppId}
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
