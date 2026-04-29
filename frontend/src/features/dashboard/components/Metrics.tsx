import React from 'react';
import type { HealthCheckLogDto } from '../../../types/dashboard.types';

interface MetricsProps {
  latestLog: HealthCheckLogDto | null;
}

const Metrics: React.FC<MetricsProps> = ({ latestLog }) => {
  // Varsayılan değerler (eğer log yoksa)
  const sysCpu = latestLog?.systemCpuUsage || 0;
  const appCpu = latestLog?.appCpuUsage || 0;
  
  const sysRam = latestLog?.systemRamUsage || 0;
  const appRam = latestLog?.appRamUsage || 0;
  
  // API'den gelen dinamik toplam kapasiteler
  const freeDisk = latestLog?.freeDiskGb || 0;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      {/* CPU Card */}
      <div className="bg-background-light border border-slate-800 rounded-xl p-5 flex flex-col justify-between shadow-lg">
        <h3 className="text-slate-100 font-semibold mb-6">CPU Kullanımı</h3>
        
        <div className="space-y-4">
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Sunucu Genel</span>
              <span className="text-slate-200 font-medium">%{sysCpu.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysCpu)}%` }}></div>
            </div>
          </div>
          
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Bu Uygulama (App CPU)</span>
              <span className="text-slate-200 font-medium">%{appCpu.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-cyan-400 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, appCpu)}%` }}></div>
            </div>
          </div>
        </div>
      </div>

      {/* RAM Card */}
      <div className="bg-background-light border border-slate-800 rounded-xl p-5 flex flex-col justify-between shadow-lg">
        <h3 className="text-slate-100 font-semibold mb-6">RAM Kullanımı</h3>
        
        <div className="space-y-4">
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Sunucu Toplam (System RAM)</span>
              <span className="text-slate-200 font-medium">%{sysRam.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysRam)}%` }}></div>
            </div>
          </div>
          
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Bu Uygulama (App RAM)</span>
              <span className="text-slate-200 font-medium">{appRam.toFixed(0)} MB</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              {/* Sadece görsel bir bar, app RAM 1024MB üzerinden % ile gösteriliyor varsayımıyla */}
              <div className="bg-purple-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, (appRam / 1024) * 100)}%` }}></div>
            </div>
          </div>
        </div>
      </div>

      {/* Disk Card */}
      <div className="bg-background-light border border-slate-800 rounded-xl p-5 flex flex-col justify-between shadow-lg">
        <h3 className="text-slate-100 font-semibold mb-6">Disk Kullanımı</h3>
        
        <div className="space-y-4">
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Kalan Boş Disk (Free Disk)</span>
              <span className="text-emerald-400 font-bold">{freeDisk.toFixed(1)} GB</span>
            </div>
          </div>
        </div>

        <div className="flex justify-between items-center mt-auto pt-4 border-t border-slate-800/50">
          <span className="text-[10px] text-slate-500 font-semibold tracking-wider">BİLGİ</span>
          <span className="text-xs text-slate-400">Veritabanından okunan boş alan</span>
        </div>
      </div>
    </div>
  );
};

export default Metrics;
