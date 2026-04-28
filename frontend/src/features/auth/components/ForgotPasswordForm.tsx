import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, ArrowLeft, Loader2, Send } from 'lucide-react';
import { authService } from '../../../api/authService';
import { toast } from 'sonner';

const forgotPasswordSchema = z.object({
  email: z.string().email('Geçerli bir e-posta giriniz'),
});

type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

interface ForgotPasswordFormProps {
  onBackToLogin: () => void;
  onSuccess: (email: string) => void;
}

export const ForgotPasswordForm: React.FC<ForgotPasswordFormProps> = ({ onBackToLogin, onSuccess }) => {
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = async (data: ForgotPasswordFormData) => {
    setErrorMsg(null);
    try {
      await authService.forgotPassword(data.email);
      toast.success('Doğrulama kodu gönderildi', {
        description: 'E-posta adresinize şifre sıfırlama kodu iletildi.',
      });
      onSuccess(data.email);
    } catch (error: any) {
      if (error.response?.data?.message) {
        setErrorMsg(error.response.data.message);
      } else {
        setErrorMsg('İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.');
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
        <h1 className="text-3xl font-bold text-white mb-2">Şifremi Unuttum</h1>
        <p className="text-slate-400">Hesabınıza kayıtlı e-posta adresini girin.</p>
      </div>

      {errorMsg && (
        <div className="mb-6 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm">
          {errorMsg}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <div className="space-y-1">
          <label className="text-sm font-medium text-slate-300 ml-1">E-posta</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <Mail size={18} />
            </div>
            <input
              {...register('email')}
              type="email"
              className="w-full pl-11 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500/50 transition-all"
              placeholder="E-posta adresinizi girin"
            />
          </div>
          {errors.email && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.email.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full mt-6 py-3 px-4 bg-emerald-500 hover:bg-emerald-600 disabled:bg-emerald-500/50 text-white rounded-xl font-medium shadow-lg shadow-emerald-500/20 transition-all flex items-center justify-center gap-2 group"
        >
          {isSubmitting ? (
            <Loader2 size={20} className="animate-spin" />
          ) : (
            <>
              Kodu Gönder
              <Send size={18} className="group-hover:translate-x-1 transition-transform" />
            </>
          )}
        </button>
      </form>
    </div>
  );
};
