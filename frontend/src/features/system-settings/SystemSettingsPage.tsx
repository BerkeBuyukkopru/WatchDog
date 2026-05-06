import React, { useState, useEffect } from 'react';
import SystemConfigForm, { type SystemConfig } from './components/SystemConfigForm';
import { systemConfigService } from '../../api/systemConfigService';
import { Settings, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

const SystemSettingsPage: React.FC = () => {
  const [config, setConfig] = useState<SystemConfig | undefined>(undefined);
  const [isFetching, setIsFetching] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);

  useEffect(() => {
    fetchConfig();
  }, []);

  const fetchConfig = async () => {
    try {
      setIsFetching(true);
      const data = await systemConfigService.getConfig();
      setConfig(data);
    } catch (error) {
      console.error('Ayarlar yüklenirken hata oluştu:', error);
      // Fallback veya Toast bildirimi yapılabilir
    } finally {
      setIsFetching(false);
    }
  };

  const handleSaveConfig = async (updatedConfig: SystemConfig) => {
    try {
      setIsSaving(true);
      await systemConfigService.updateConfig(updatedConfig);
      setConfig(updatedConfig);
      toast.success('Sistem ayarları başarıyla güncellendi!');
    } catch (error) {
      console.error('Ayarlar kaydedilirken hata oluştu:', error);
      toast.error('Ayarlar kaydedilirken bir hata oluştu.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="flex flex-col gap-6 p-4 md:p-6 w-full max-w-5xl mx-auto">
      {/* Page Header */}
      <div className="flex items-center gap-4 border-b border-slate-800 pb-4">
        <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-500">
          <Settings size={24} />
        </div>
        <div>
          <h1 className="text-xl font-black text-slate-100 uppercase tracking-widest">Sistem Konfigürasyonu</h1>
          <p className="text-xs text-slate-500 font-medium mt-1">Watchdog tarama motoru ve eşik değerlerini yapılandırın.</p>
        </div>
      </div>

      {/* Content */}
      <div className="w-full">
        {isFetching ? (
          <div className="flex flex-col items-center justify-center py-20 text-slate-500 gap-3">
            <Loader2 size={32} className="animate-spin text-indigo-500" />
            <span className="text-sm font-semibold animate-pulse">Sistem ayarları yükleniyor...</span>
          </div>
        ) : (
          <SystemConfigForm 
            initialData={config} 
            onSubmit={handleSaveConfig} 
            isLoading={isSaving} 
          />
        )}
      </div>
    </div>
  );
};

export default SystemSettingsPage;
