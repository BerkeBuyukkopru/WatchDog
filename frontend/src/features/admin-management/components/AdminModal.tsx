import React, { useState, useEffect } from 'react';
import { X, Shield, Mail, User, Loader2, Check, Plus } from 'lucide-react';
import { adminService } from '../../../api/adminService';
import type { AdminUser, MonitoredApp } from '../../../types/admin.types';
import { toast } from 'sonner';

interface AdminModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  admin: AdminUser | null;
  availableApps: MonitoredApp[];
}

export const AdminModal: React.FC<AdminModalProps> = ({ isOpen, onClose, onSuccess, admin, availableApps }) => {
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    role: 'Admin',
    password: '',
    allowedAppIds: [] as string[]
  });

  useEffect(() => {
    if (admin) {
      setFormData({
        username: admin.username,
        email: admin.email,
        role: admin.role,
        password: '', // Password not shown on edit
        allowedAppIds: admin.allowedAppIds || []
      });
    } else {
      setFormData({
        username: '',
        email: '',
        role: 'Admin',
        password: '',
        allowedAppIds: []
      });
    }
  }, [admin, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (admin) {
        await adminService.updateAdmin(admin.id, {
          username: formData.username,
          email: formData.email,
          role: formData.role,
          allowedAppIds: formData.allowedAppIds
        });
        toast.success('Yönetici başarıyla güncellendi');
      } else {
        await adminService.createAdmin({
          username: formData.username,
          email: formData.email,
          role: formData.role,
          password: formData.password,
          allowedAppIds: formData.allowedAppIds
        });
        toast.success('Yeni yönetici oluşturuldu');
      }
      onSuccess();
      onClose();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Bir hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  const toggleApp = (appId: string) => {
    setFormData(prev => ({
      ...prev,
      allowedAppIds: prev.allowedAppIds.includes(appId)
        ? prev.allowedAppIds.filter(id => id !== appId)
        : [...prev.allowedAppIds, appId]
    }));
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-[#16161A] border border-white/10 rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="p-6 border-b border-white/5 flex items-center justify-between bg-white/5">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-500/20 rounded-lg text-indigo-400">
              <Shield size={20} />
            </div>
            <h2 className="text-xl font-bold text-white">
              {admin ? 'Yöneticiyi Düzenle' : 'Yeni Admin Ekle'}
            </h2>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-white/10 rounded-full text-slate-400 transition-colors">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">İsim Soyisim</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <User size={16} />
                </div>
                <input
                  type="text"
                  required
                  value={formData.username}
                  onChange={e => setFormData({ ...formData, username: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                  placeholder="Örn: Ali Veli"
                />
              </div>
            </div>
            <div className="space-y-1.5">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">Email Adresi</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
                  <Mail size={16} />
                </div>
                <input
                  type="email"
                  required
                  value={formData.email}
                  onChange={e => setFormData({ ...formData, email: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                  placeholder="ornek@sirket.com"
                />
              </div>
            </div>
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">Rol Seçimi</label>
            <select
              value={formData.role}
              onChange={e => setFormData({ ...formData, role: e.target.value })}
              className="w-full px-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all appearance-none cursor-pointer"
            >
              <option value="Admin">Admin (Sınırlı Erişim)</option>
              <option value="SuperAdmin">SuperAdmin (Tam Erişim)</option>
            </select>
          </div>

          {!admin && (
            <div className="space-y-1.5">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest ml-1">Geçici Şifre</label>
              <input
                type="password"
                required
                value={formData.password}
                onChange={e => setFormData({ ...formData, password: e.target.value })}
                className="w-full px-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all"
                placeholder="••••••••"
              />
            </div>
          )}

          <div className="space-y-4">
            <div className="flex items-center justify-between px-1">
              <label className="text-xs font-black text-slate-500 uppercase tracking-widest">Yetkili Olduğu Uygulamalar</label>
              <span className="text-[10px] font-bold text-indigo-400 bg-indigo-500/10 px-2 py-0.5 rounded-full border border-indigo-500/20">
                {formData.allowedAppIds.length} Aktif Yetki
              </span>
            </div>
            
            <div className="space-y-4 max-h-64 overflow-y-auto custom-scrollbar pr-2">
              {/* GRUP 1: HALİHAZIRDA YETKİLİ OLANLAR */}
              {availableApps.filter(app => formData.allowedAppIds.includes(app.id)).length > 0 && (
                <div className="space-y-2">
                  <div className="grid grid-cols-1 gap-2">
                    {availableApps.filter(app => formData.allowedAppIds.includes(app.id)).map(app => (
                      <button
                        key={app.id}
                        type="button"
                        onClick={() => toggleApp(app.id)}
                        className="group flex items-center justify-between p-3 bg-indigo-500/5 border border-indigo-500/20 rounded-xl hover:bg-indigo-500/10 transition-all text-left"
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-5 h-5 rounded-md bg-indigo-500 flex items-center justify-center text-white shadow-lg shadow-indigo-500/20">
                            <Check size={12} strokeWidth={4} />
                          </div>
                          <span className="text-xs font-bold text-indigo-100">{app.name}</span>
                        </div>
                        <span className="text-[9px] font-black text-indigo-500/60 group-hover:text-rose-500 transition-colors">KALDIR</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* GRUP 2: YENİ EKLEBİLECEKLERİ */}
              <div className="space-y-3 pt-2 border-t border-white/5">
                <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest ml-1 flex items-center gap-2">
                  <Plus size={12} className="text-indigo-500" />
                  Yeni Uygulama Ekle
                </label>
                <div className="grid grid-cols-1 gap-2">
                  {availableApps.filter(app => !formData.allowedAppIds.includes(app.id)).map(app => (
                    <button
                      key={app.id}
                      type="button"
                      onClick={() => toggleApp(app.id)}
                      className="group flex items-center justify-between p-3 bg-white/[0.03] border border-white/10 rounded-xl hover:bg-white/5 hover:border-indigo-500/30 transition-all text-left"
                    >
                      <div className="flex items-center gap-3">
                        <div className="shrink-0 w-5 h-5 rounded-md bg-white/10 border border-white/10 flex items-center justify-center group-hover:bg-indigo-500/20 group-hover:border-indigo-500/50 transition-all">
                          <Plus size={12} className="text-slate-400 group-hover:text-indigo-400" />
                        </div>
                        <span className="text-xs font-bold text-slate-400 group-hover:text-slate-200 transition-colors">{app.name}</span>
                      </div>
                      <span className="text-[9px] font-black text-slate-600 group-hover:text-indigo-400 transition-colors uppercase tracking-widest">EKLE</span>
                    </button>
                  ))}
                </div>
              </div>
            </div>

            <p className="text-[10px] text-slate-500 px-2 flex items-center gap-1.5 italic">
              <span className="w-1 h-1 rounded-full bg-indigo-500"></span>
              Seçilen uygulamalar bu yöneticinin gözetim paneline anında eklenecektir.
            </p>
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
              {admin ? 'Güncelle' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
