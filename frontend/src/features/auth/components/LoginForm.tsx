import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, Lock, Loader2, ArrowRight } from 'lucide-react';
import { authService } from '../../../api/authService';
import { useAuth } from '../../../context/AuthContext';

const loginSchema = z.object({
  username: z.string().min(1, 'Kullanıcı adı zorunludur'),
  password: z.string().min(1, 'Şifre zorunludur'),
});

type LoginFormData = z.infer<typeof loginSchema>;

interface LoginFormProps {
  onForgotPassword: () => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onForgotPassword }) => {
  const { login } = useAuth();
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setErrorMsg(null);
    try {
      const response = await authService.login(data);
      if (response && response.token) {
        login(response.token);
      }
    } catch (error: any) {
      if (error.response?.data?.message) {
        setErrorMsg(error.response.data.message);
      } else {
        setErrorMsg('Giriş yapılırken bir hata oluştu. Lütfen tekrar deneyin.');
      }
    }
  };

  return (
    <div className="w-full">
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold text-white mb-2">Hoş Geldiniz</h1>
        <p className="text-slate-400">Sistem Sağlık İzleme Monitörü</p>
      </div>

      {errorMsg && (
        <div className="mb-6 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm">
          {errorMsg}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <div className="space-y-1">
          <label className="text-sm font-medium text-slate-300 ml-1">Kullanıcı Adı</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <Mail size={18} />
            </div>
            <input
              {...register('username')}
              type="text"
              className="w-full pl-11 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500/50 transition-all"
              placeholder="admin"
            />
          </div>
          {errors.username && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.username.message}</p>}
        </div>

        <div className="space-y-1">
          <div className="flex justify-between items-center ml-1">
            <label className="text-sm font-medium text-slate-300">Şifre</label>
            <button
              type="button"
              onClick={onForgotPassword}
              className="text-xs font-medium text-emerald-400 hover:text-emerald-300 transition-colors"
            >
              Şifremi Unuttum
            </button>
          </div>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-500">
              <Lock size={18} />
            </div>
            <input
              {...register('password')}
              type="password"
              className="w-full pl-11 pr-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500/50 transition-all"
              placeholder="••••••••"
            />
          </div>
          {errors.password && <p className="text-rose-400 text-xs mt-1 ml-1">{errors.password.message}</p>}
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
              Sisteme Giriş Yap
              <ArrowRight size={18} className="group-hover:translate-x-1 transition-transform" />
            </>
          )}
        </button>
      </form>
    </div>
  );
};
