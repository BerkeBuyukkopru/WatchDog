import { useState, useEffect } from 'react';
import { adminService } from '../../../api/adminService';
import type { AdminUser, MonitoredApp } from '../../../types/admin.types';
import { toast } from 'sonner';

export const useAdminManagement = (activeTab: 'active' | 'deleted') => {
  const [admins, setAdmins] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [availableApps, setAvailableApps] = useState<MonitoredApp[]>([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [activeAdmins, deletedAdmins, appsData] = await Promise.all([
        adminService.getAdmins(),
        adminService.getDeletedAdmins(),
        adminService.getAvailableApps()
      ]);
      
      setAdmins(activeTab === 'active' ? activeAdmins : deletedAdmins);
      setAvailableApps(appsData);
    } catch (error) {
      toast.error('Veriler yüklenirken bir hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const deleteAdmin = async (id: string) => {
    try {
      await adminService.deleteAdmin(id);
      toast.success('Yönetici başarıyla silindi');
      loadData();
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || error.response?.data?.Message || 'Silme işlemi başarısız';
      toast.error(errorMsg);
    }
  };

  const restoreAdmin = async (id: string) => {
    try {
      await adminService.restoreAdmin(id);
      toast.success('Yönetici başarıyla geri yüklendi');
      loadData();
    } catch (error) {
      toast.error('Geri yükleme başarısız');
    }
  };

  return {
    admins,
    loading,
    availableApps,
    refresh: loadData,
    deleteAdmin,
    restoreAdmin
  };
};
