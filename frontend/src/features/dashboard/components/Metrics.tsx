import React from 'react';
import type { HealthCheckLogDto } from '../../../types/dashboard.types';

interface MetricsProps {
  latestLog: HealthCheckLogDto | null;
  appName?: string;
}

const Metrics: React.FC<MetricsProps> = ({ latestLog, appName }) => {
  const sysCpu = latestLog?.systemCpuUsage || 0;
  const appCpu = latestLog?.appCpuUsage || 0;
  const sysRam = latestLog?.systemRamUsage || 0;
  const appRam = latestLog?.appRamUsage || 0;
  const freeDisk = latestLog?.freeDiskGb || 0;
  const totalDisk = latestLog?.totalDiskGb || 100;
  const totalRam = latestLog?.totalRamMb || 16384;

  const diskUsagePercent = ((totalDisk - freeDisk) / totalDisk) * 100;
  const appRamPercent = (appRam / totalRam) * 100;

  return (
    <div className="flex flex-col gap-4">
      {appName && (
        <div className="flex items-center gap-2 px-1 mb-1">
          <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse shadow-[0_0_8px_rgba(16,185,129,0.5)]"></div>
          <span className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">
            Metrik İzleme: <span className="text-slate-100">{appName}</span>
          </span>
        </div>
      )}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* CPU Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-xs font-bold mb-3 uppercase tracking-wider">CPU Kullanımı</h3>
          <div className="space-y-2.5">
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Sistem CPU Kullanımı (System CPU Usage)</span>
              <span className="text-slate-200 font-medium">%{sysCpu.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysCpu)}%` }}></div>
            </div>
          </div>
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Uygulama CPU Kullanımı (App CPU Usage)</span>
              <span className="text-slate-200 font-medium">%{appCpu.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-cyan-400 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, appCpu)}%` }}></div>
            </div>
          </div>
        </div>
      </div>

        {/* RAM Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-xs font-bold mb-3 uppercase tracking-wider">RAM Kullanımı</h3>
          <div className="space-y-2.5">
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Sistem RAM Kullanımı (System RAM Usage)</span>
              <span className="text-slate-200 font-medium">%{sysRam.toFixed(1)}</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysRam)}%` }}></div>
            </div>
          </div>
          <div>
            <div className="flex justify-between text-xs mb-1">
              <span className="text-slate-400">Uygulama RAM Kullanımı (App RAM Usage)</span>
              <span className="text-slate-200 font-medium">{appRam.toFixed(0)} MB</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-1.5">
              <div className="bg-purple-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, appRamPercent)}%` }}></div>
            </div>
          </div>
        </div>
      </div>

        {/* Disk Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-xs font-bold mb-3 uppercase tracking-wider">Disk Kullanımı</h3>
          <div className="space-y-2.5">
            <div>
              <div className="flex justify-between text-xs mb-1">
                <span className="text-slate-400">Doluluk Oranı (Disk Usage)</span>
                <span className="text-emerald-400 font-bold">%{diskUsagePercent.toFixed(1)}</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-emerald-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, diskUsagePercent)}%` }}></div>
              </div>
              <div className="mt-1.5 text-[10px] text-slate-500 flex justify-between">
                <span>Boş: {freeDisk.toFixed(1)} GB</span>
                <span>Toplam: {totalDisk.toFixed(0)} GB</span>
              </div>
            </div>
          </div>
          <div className="flex justify-between items-center mt-auto pt-3 border-t border-slate-800/50">
            <span className="text-[9px] text-slate-500 font-semibold tracking-wider">CANLI VERİ</span>
            <span className="text-[10px] text-slate-400">Depolama Durumu</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Metrics;
