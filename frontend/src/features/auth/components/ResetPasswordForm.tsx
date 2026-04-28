import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Lock, KeyRound, Loader2, ArrowLeft, CheckCircle } from 'lucide-react';
import { authService } from '../../../api/authService';
import { toast } from 'sonner';

const resetPasswordSchema = z.object({
  resetCode: z.string().min(6, 'Doğrulama kodu en az 6 karakter olmalıdır'),
  newPassword: z.string()
    .min(8, 'Şifre en az 8 karakter olmalıdır')
    .regex(/[A-Z]/, 'En az 1 büyük harf içermelidir')
    .regex(/[0-9]/, 'En az 1 rakam içermelidir')
    .regex(/[^a-zA-Z0-9]/, 'En az 1 özel karakter içermelidir'),
  confirmPassword: z.string().min(1, 'Lütfen şifrenizi tekrar girin'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Şifreler eşleşmiyor',
  path: ['confirmPassword'],
});

type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

interface ResetPasswordFormProps {
  email: string;
  onBackToLogin: () => void;
}

export const ResetPasswordForm: React.FC<ResetPasswordFormProps> = ({ email, onBackToLogin }) => {
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmit = async (data: ResetPasswordFormData) => {
    setErrorMsg(null);
    try {
      await authService.resetPassword({
        Email: email,
        ResetCode: data.resetCode,
        NewPassword: data.newPassword,
      });
      
      toast.success('Şifre Başarıyla Değiştirildi', {
        description: 'Yeni şifrenizle giriş yapabilirsiniz.',
      });
      
      // Navigate back to login
      setTimeout(() => {
        onBackToLogin();
      }, 1500);
      
    } catch (error: any) {
      if (error.response?.data?.message) {
        setErrorMsg(error.response.data.message);
      } else {
        setErrorMsg('Şifre sıfırlanırken bir hata oluştu. Lütfen kodunuzu kontrol edip tekrar deneyin.');
      }
    }
  };

  return (
    <div className="w-full">
      <button
        onClick={onBackToLogin}
        className="flex items-center text-sm font-medium text-slate-400 hover:text-white transition-colors mb-6 group"
      >
        <ArrowLeft size={16} className="mr-2 group-hover:-translate-x-1 transition-transform" />
        Giriş Ekranına Dön
      </button>

      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold text-white mb-2">Yeni Şifre Belirle</h1>
        <p className="text-slate-400 text-sm">
          <span className="text-emerald-400 font-medium">{email}</span> adresine gönderilen kodu girin.
        </p>
      </div>

      {errorMsg && (
        <div className="mb-6 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm">
          {errorMsg}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <div className="space-y-1">
          <label className="text-sm font-medium text-slate-300 ml-1">Doğrulama Kodu</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <KeyRound size={18} />
            </div>
            <input
              {...register('resetCode')}
              type="text"
              className={`w-full pl-11 pr-4 py-3 bg-white/5 border rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 transition-all ${
                errors.resetCode 
                  ? 'border-rose-500/50 focus:ring-rose-500/50 focus:border-rose-500/50' 
                  : 'border-white/10 focus:ring-emerald-500/50 focus:border-emerald-500/50'
              }`}
              placeholder="123456"
            />
          </div>
          {errors.resetCode && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.resetCode.message}</p>}
        </div>

        <div className="space-y-1">
          <label className="text-sm font-medium text-slate-300 ml-1">Yeni Şifre</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <Lock size={18} />
            </div>
            <input
              {...register('newPassword')}
              type="password"
              className={`w-full pl-11 pr-4 py-3 bg-white/5 border rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 transition-all ${
                errors.newPassword 
                  ? 'border-rose-500/50 focus:ring-rose-500/50 focus:border-rose-500/50' 
                  : 'border-white/10 focus:ring-emerald-500/50 focus:border-emerald-500/50'
              }`}
              placeholder="••••••••"
            />
          </div>
          {errors.newPassword && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.newPassword.message}</p>}
        </div>

        <div className="space-y-1">
          <label className="text-sm font-medium text-slate-300 ml-1">Yeni Şifre (Tekrar)</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <CheckCircle size={18} />
            </div>
            <input
              {...register('confirmPassword')}
              type="password"
              className={`w-full pl-11 pr-4 py-3 bg-white/5 border rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 transition-all ${
                errors.confirmPassword 
                  ? 'border-rose-500/50 focus:ring-rose-500/50 focus:border-rose-500/50' 
                  : 'border-white/10 focus:ring-emerald-500/50 focus:border-emerald-500/50'
              }`}
              placeholder="••••••••"
            />
          </div>
          {errors.confirmPassword && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.confirmPassword.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full mt-6 py-3 px-4 bg-emerald-500 hover:bg-emerald-600 disabled:bg-emerald-500/50 text-white rounded-xl font-medium shadow-lg shadow-emerald-500/20 transition-all flex items-center justify-center gap-2"
        >
          {isSubmitting ? (
            <Loader2 size={20} className="animate-spin" />
          ) : (
            'Şifreyi Güncelle'
          )}
        </button>
      </form>
    </div>
  );
};
