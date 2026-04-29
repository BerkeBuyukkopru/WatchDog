import React from 'react';
import { AiTower } from '../features/ai-tower/components/AiTower';

export const DashboardPage: React.FC = () => {
  return (
    <div className="flex h-full gap-0 overflow-hidden bg-background-darker">
      {/* Metrics Area (Left 70%) - To be implemented by team member */}
      <div className="flex-1 overflow-auto p-4 sm:p-6 custom-scrollbar">
        <div className="max-w-7xl mx-auto space-y-6">
          <div className="flex flex-col gap-2">
            <h1 className="text-2xl font-bold text-white">Sistem Genel Bakış</h1>
            <p className="text-slate-400 text-sm">Canlı metrikler ve uygulama sağlık durumu</p>
          </div>
          
          {/* Placeholder for metrics */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="h-40 bg-white/5 border border-white/10 rounded-2xl animate-pulse flex items-center justify-center text-slate-500 italic">
              Metrik Paneli (Geliştirici A)
            </div>
            <div className="h-40 bg-white/5 border border-white/10 rounded-2xl animate-pulse flex items-center justify-center text-slate-500 italic">
              Ağ Trafiği (Geliştirici A)
            </div>
            <div className="h-40 bg-white/5 border border-white/10 rounded-2xl animate-pulse flex items-center justify-center text-slate-500 italic">
              Hata Oranları (Geliştirici A)
            </div>
          </div>
          
          <div className="h-96 bg-white/5 border border-white/10 rounded-2xl animate-pulse flex items-center justify-center text-slate-500 italic text-xl">
            Sistem Grafikleri ve Uygulama Listesi (Geliştirici A)
          </div>
        </div>
      </div>

      {/* AI Insight Tower (Right 30%) */}
      <div className="w-[30%] min-w-[320px] max-w-sm hidden xl:block shrink-0">
        <AiTower />
      </div>
    </div>
  );
};

export default DashboardPage;
