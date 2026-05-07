import { LayoutGrid, CheckCircle2, ShieldAlert, AlertTriangle } from 'lucide-react';

interface SummaryCardsProps {
  totalApps: number;
  healthyApps: number;
  unhealthyApps: number;
  degradedApps: number;
}

const SummaryCards: React.FC<SummaryCardsProps> = ({ totalApps, healthyApps, unhealthyApps, degradedApps }) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-2">
      {/* Toplam Uygulama */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-10 h-10 rounded-xl bg-blue-500/10 flex items-center justify-center text-blue-500 shrink-0">
          <LayoutGrid size={20} />
        </div>
        <div>
          <p className="text-[9px] font-black text-slate-500 uppercase tracking-widest">Toplam</p>
          <h3 className="text-xl font-black text-slate-100">{totalApps}</h3>
        </div>
      </div>

      {/* Sağlıklı Uygulamalar */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500 shrink-0">
          <CheckCircle2 size={20} />
        </div>
        <div>
          <p className="text-[9px] font-black text-slate-500 uppercase tracking-widest">Sağlıklı</p>
          <h3 className="text-xl font-black text-emerald-400">{healthyApps}</h3>
        </div>
      </div>

      {/* Degraded Uygulamalar */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500 shrink-0">
          <AlertTriangle size={20} />
        </div>
        <div>
          <p className="text-[9px] font-black text-slate-500 uppercase tracking-widest">Kısıtlı (Degraded)</p>
          <h3 className="text-xl font-black text-amber-400">{degradedApps}</h3>
        </div>
      </div>

      {/* Hatalı Uygulamalar */}
      <div className="bg-background-light border border-slate-800 rounded-2xl p-4 flex items-center gap-4 shadow-lg transition-all hover:border-slate-700">
        <div className="w-10 h-10 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500 shrink-0">
          <ShieldAlert size={20} />
        </div>
        <div>
          <p className="text-[9px] font-black text-slate-500 uppercase tracking-widest">Kritik Hata</p>
          <h3 className="text-xl font-black text-rose-400">{unhealthyApps}</h3>
        </div>
      </div>
    </div>
  );
};

export default SummaryCards;
