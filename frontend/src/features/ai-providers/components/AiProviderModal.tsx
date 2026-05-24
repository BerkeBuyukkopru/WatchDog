import React, { useState, useEffect } from 'react';
import { X, BrainCircuit, Globe, Lock, Loader2, Sparkles } from 'lucide-react';
import { aiProviderService } from '../../../api/aiProviderService';
import { getApiErrorMessage } from '../../../api/apiError';
import type { AiProviderDetail } from '../../../types/ai-provider.types';
import { toast } from 'sonner';

interface AiProviderModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  provider: AiProviderDetail | null;
}

export const AiProviderModal: React.FC<AiProviderModalProps> = ({ isOpen, onClose, onSuccess, provider }) => {
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    modelName: '',
    apiUrl: '',
    apiKey: '',
    isActive: true
  });

  useEffect(() => {
    if (provider) {
      setFormData({
        name: provider.name,
        modelName: provider.modelName,
        apiUrl: provider.apiUrl,
        apiKey: '', // Keep empty on edit unless changing
        isActive: provider.isActive
      });
    } else {
      setFormData({
        name: '',
        modelName: '',
        apiUrl: '',
        apiKey: '',
        isActive: true
      });
    }
  }, [provider, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (provider) {
        await aiProviderService.updateProvider(provider.id, {
          name: formData.name,
          modelName: formData.modelName,
          apiUrl: formData.apiUrl,
          apiKey: formData.apiKey || undefined,
          isActive: formData.isActive
        });
        toast.success('Sağlayıcı başarıyla güncellendi');
      } else {
        await aiProviderService.createProvider(formData);
        toast.success('Yeni sağlayıcı oluşturuldu');
      }
      onSuccess();
      onClose();
    } catch (error) {
      toast.error(getApiErrorMessage(error, 'Bir hata oluştu'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed top-0 left-0 w-full h-full z-[9999] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/80 backdrop-blur-sm animate-in fade-in duration-300" 
        onClick={onClose}
      />

      {/* Modal Box */}
      <div className="relative z-10 bg-[#16161A] border border-white/10 rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200 max-h-[95vh] flex flex-col">
        <div className="p-6 border-b border-white/5 flex items-center justify-between bg-white/5 shrink-0">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-500/20 rounded-lg text-indigo-400">
              <Sparkles size={20} />
            </div>
            <h2 className="text-xl font-bold text-white">
              {provider ? 'AI Sağlayıcısını Düzenle' : 'Yeni AI Sağlayıcısı Ekle'}
            </h2>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-white/10 rounded-full text-slate-400 transition-colors">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5 overflow-y-auto custom-scrollbar">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">Sağlayıcı Adı</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <BrainCircuit size={16} />
                </div>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={e => setFormData({ ...formData, name: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                  placeholder="Örn: OpenAI, Anthropic"
                />
              </div>
            </div>
            <div className="space-y-1.5">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">Model Adı</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <Sparkles size={16} />
                </div>
                <input
                  type="text"
                  required
                  value={formData.modelName}
                  onChange={e => setFormData({ ...formData, modelName: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                  placeholder="Örn: gpt-4-turbo"
                />
              </div>
            </div>
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">API Endpoint URL</label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                <Globe size={16} />
              </div>
              <input
                type="url"
                required
                value={formData.apiUrl}
                onChange={e => setFormData({ ...formData, apiUrl: e.target.value })}
                className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                placeholder="https://api.openai.com/v1"
              />
            </div>
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
              API Key {provider && <span className="text-indigo-400 capitalize">(Opsiyonel - Değiştirmek için yazın)</span>}
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                <Lock size={16} />
              </div>
              <input
                type="password"
                required={!provider}
                value={formData.apiKey}
                onChange={e => setFormData({ ...formData, apiKey: e.target.value })}
                className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                placeholder={provider ? '••••••••••••••••' : 'sk-...'}
              />
            </div>
            <p className="text-[10px] text-slate-500 px-1 italic">API anahtarı sistem veritabanında şifrelenerek saklanacaktır.</p>
          </div>

          <div className="flex items-center gap-3 px-1">
            <button
              type="button"
              onClick={() => setFormData({ ...formData, isActive: !formData.isActive })}
              className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none ${
                formData.isActive ? 'bg-emerald-500' : 'bg-slate-700'
              }`}
            >
              <span
                className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                  formData.isActive ? 'translate-x-6' : 'translate-x-1'
                }`}
              />
            </button>
            <span className="text-xs font-bold text-slate-300 uppercase tracking-wider">
              {formData.isActive ? 'Sağlayıcı Aktif' : 'Sağlayıcı Pasif'}
            </span>
          </div>

          <div className="pt-4 border-t border-white/5 flex justify-end gap-3 bg-white/5 -mx-6 -mb-6 p-6 mt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-6 py-2.5 bg-white/5 hover:bg-white/10 text-slate-300 text-sm font-bold rounded-xl transition-all"
            >
              İptal
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-8 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-bold rounded-xl transition-all shadow-lg shadow-indigo-600/20 flex items-center gap-2"
            >
              {loading && <Loader2 size={16} className="animate-spin" />}
              {provider ? 'Güncelle' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
