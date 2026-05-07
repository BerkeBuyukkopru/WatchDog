import { useState, useEffect } from 'react';
import { aiProviderService } from '../../../api/aiProviderService';
import type { AiProviderDetail } from '../../../types/ai-provider.types';
import { toast } from 'sonner';

export const useAiProviders = (activeTab: 'active' | 'deleted') => {
  const [providers, setProviders] = useState<AiProviderDetail[]>([]);
  const [loading, setLoading] = useState(true);

  const loadData = async () => {
    setLoading(true);
    try {
      const data = activeTab === 'active' 
        ? await aiProviderService.getProviders()
        : await aiProviderService.getDeletedProviders();
      setProviders(data);
    } catch (error) {
      toast.error('Sağlayıcılar yüklenirken bir hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const toggleStatus = async (id: string) => {
    try {
      await aiProviderService.toggleStatus(id);
      toast.success('Sağlayıcı durumu güncellendi');
      loadData();
    } catch (error) {
      toast.error('Durum güncellenemedi');
    }
  };

  const deleteProvider = async (id: string) => {
    try {
      await aiProviderService.deleteProvider(id);
      toast.success('Sağlayıcı donduruldu (Silinenlere taşındı)');
      loadData();
    } catch (error) {
      toast.error('Silme işlemi başarısız');
    }
  };

  const restoreProvider = async (id: string) => {
    try {
      await aiProviderService.restoreProvider(id);
      toast.success('Sağlayıcı başarıyla geri yüklendi');
      loadData();
    } catch (error) {
      toast.error('Geri yükleme işlemi başarısız');
    }
  };

  return {
    providers,
    loading,
    refresh: loadData,
    toggleStatus,
    deleteProvider,
    restoreProvider
  };
};
