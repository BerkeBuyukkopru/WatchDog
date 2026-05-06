import React, { useState, useEffect } from 'react';
import { X, Globe, Link2, Clock, Save } from 'lucide-react';

export interface AppFormData {
  name: string;
  healthUrl: string;
  pollingIntervalSeconds: number;
  isActive: boolean;
}

interface AppFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: AppFormData) => void;
  initialData?: AppFormData;
  isEdit?: boolean;
}

const AppFormModal: React.FC<AppFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  initialData,
  isEdit = false
}) => {
  const [formData, setFormData] = useState<AppFormData>({
    name: '',
    healthUrl: '',
    pollingIntervalSeconds: 60,
    isActive: true
  });

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    } else {
      setFormData({ 
        name: '', 
        healthUrl: '', 
        pollingIntervalSeconds: 60, 
        isActive: true 
      });
    }
  }, [initialData, isOpen]);

  if (!isOpen) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'pollingIntervalSeconds' ? Number(value) : value
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm transition-opacity" 
        onClick={onClose}
      />

      {/* Modal Container */}
      <div className="relative w-full max-w-lg bg-background-light border border-slate-800 rounded-2xl shadow-2xl overflow-hidden animate-in fade-in zoom-in duration-200">
        
        {/* Header */}
        <div className="h-[60px] px-6 border-b border-slate-800 flex items-center justify-between bg-slate-800/10">
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${isEdit ? 'bg-amber-500/10 text-amber-500' : 'bg-emerald-500/10 text-emerald-500'}`}>
              <Globe size={18} />
            </div>
            <h2 className="text-sm font-black text-slate-100 uppercase tracking-[0.2em]">
              {isEdit ? 'Uygulamayı Düzenle' : 'Yeni Uygulama Ekle'}
            </h2>
          </div>
          <button 
            onClick={onClose}
            className="p-2 text-slate-500 hover:text-slate-300 hover:bg-slate-800 rounded-xl transition-all"
          >
            <X size={20} />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* Uygulama Adı */}
          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Uygulama Adı</label>
            <div className="relative">
              <Globe size={14} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="text"
                name="name"
                required
                value={formData.name}
                onChange={handleChange}
                placeholder="Örn: OrderStream API"
                className="w-full bg-slate-900/50 border border-slate-800 rounded-xl py-2.5 pl-10 pr-4 text-sm text-slate-200 placeholder:text-slate-700 focus:outline-none focus:ring-1 focus:ring-indigo-500/50 focus:border-indigo-500/50 transition-all"
              />
            </div>
          </div>

          {/* URL */}
          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Uygulama URL</label>
            <div className="relative">
              <Link2 size={14} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="url"
                name="healthUrl"
                required
                value={formData.healthUrl}
                onChange={handleChange}
                placeholder="https://api.example.com/health"
                className="w-full bg-slate-900/50 border border-slate-800 rounded-xl py-2.5 pl-10 pr-4 text-sm text-slate-200 placeholder:text-slate-700 focus:outline-none focus:ring-1 focus:ring-indigo-500/50 focus:border-indigo-500/50 transition-all"
              />
            </div>
          </div>

          {/* Tarama Sıklığı (Saniye) */}
          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Tarama Sıklığı (Sn)</label>
            <div className="relative">
              <Clock size={14} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500 pointer-events-none" />
              <input
                type="number"
                name="pollingIntervalSeconds"
                min={10}
                max={3600}
                required
                value={formData.pollingIntervalSeconds}
                onChange={handleChange}
                className="w-full bg-slate-900/50 border border-slate-800 rounded-xl py-2.5 pl-10 pr-4 text-sm text-slate-200 focus:outline-none focus:ring-1 focus:ring-indigo-500/50 focus:border-indigo-500/50 transition-all"
              />
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex items-center justify-end gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-6 py-2.5 text-xs font-black text-slate-500 uppercase tracking-widest hover:text-slate-300 transition-colors"
            >
              İptal
            </button>
            <button
              type="submit"
              className={`flex items-center gap-2 px-8 py-2.5 rounded-xl text-white text-xs font-black uppercase tracking-widest transition-all shadow-lg active:scale-95 ${
                isEdit 
                ? 'bg-amber-600 hover:bg-amber-500 shadow-amber-900/20' 
                : 'bg-emerald-600 hover:bg-emerald-500 shadow-emerald-900/20'
              }`}
            >
              <Save size={16} />
              {isEdit ? 'Güncelle' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AppFormModal;
