import React from 'react';
import { LayoutGrid, CheckCircle2, ShieldAlert } from 'lucide-react';

interface SummaryCardsProps {
  totalApps: number;
  healthyApps: number;
  unhealthyApps: number;
}

const SummaryCards: React.FC<SummaryCardsProps> = ({ totalApps, healthyApps, unhealthyApps }) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-2">
      {/* Toplam Uygulama */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-12 h-12 rounded-xl bg-blue-500/10 flex items-center justify-center text-blue-500">
          <LayoutGrid size={24} />
        </div>
        <div>
          <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Toplam Uygulama</p>
          <h3 className="text-2xl font-black text-slate-100">{totalApps}</h3>
        </div>
      </div>

      {/* Sağlıklı Uygulamalar */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
          <CheckCircle2 size={24} />
        </div>
        <div>
          <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Sağlıklı Sistem</p>
          <h3 className="text-2xl font-black text-emerald-400">{healthyApps}</h3>
        </div>
      </div>

      {/* Hatalı/Degraded Uygulamalar */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
          <ShieldAlert size={24} />
        </div>
        <div>
          <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Kritik Uyarılar</p>
          <h3 className="text-2xl font-black text-rose-400">{unhealthyApps}</h3>
        </div>
      </div>
    </div>
  );
};

export default SummaryCards;
