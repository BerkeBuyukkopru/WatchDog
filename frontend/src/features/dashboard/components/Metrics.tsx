import React from 'react';
import type { HealthCheckLogDto } from '../../../types/dashboard.types';

interface MetricsProps {
  latestLog: HealthCheckLogDto | null;
}

const Metrics: React.FC<MetricsProps> = ({ latestLog }) => {
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
    <div className="flex flex-col gap-3">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 sm:gap-4">
        {/* CPU Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-3 sm:p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-[10px] sm:text-xs font-bold mb-2 sm:mb-3 uppercase tracking-wider">CPU Kullanımı</h3>
          <div className="space-y-3">
            <div>
              <div className="flex justify-between text-[10px] sm:text-xs mb-1">
                <span className="text-slate-400 truncate mr-2">Sistem CPU <span className="hidden xs:inline">Kullanımı</span></span>
                <span className="text-slate-200 font-bold shrink-0">%{sysCpu.toFixed(1)}</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysCpu)}%` }}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between text-[10px] sm:text-xs mb-1">
                <span className="text-slate-400 truncate mr-2">Uygulama CPU <span className="hidden xs:inline">Kullanımı</span></span>
                <span className="text-slate-200 font-bold shrink-0">%{appCpu.toFixed(1)}</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-cyan-400 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, appCpu)}%` }}></div>
              </div>
            </div>
          </div>
        </div>

        {/* RAM Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-3 sm:p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-[10px] sm:text-xs font-bold mb-2 sm:mb-3 uppercase tracking-wider">RAM Kullanımı</h3>
          <div className="space-y-3">
            <div>
              <div className="flex justify-between text-[10px] sm:text-xs mb-1">
                <span className="text-slate-400 truncate mr-2">Sistem RAM <span className="hidden xs:inline">Kullanımı</span></span>
                <span className="text-slate-200 font-bold shrink-0">%{sysRam.toFixed(1)}</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-blue-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, sysRam)}%` }}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between text-[10px] sm:text-xs mb-1">
                <span className="text-slate-400 truncate mr-2">Uygulama RAM <span className="hidden xs:inline">Kullanımı</span></span>
                <span className="text-slate-200 font-bold shrink-0">{appRam.toFixed(0)} MB</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-purple-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, appRamPercent)}%` }}></div>
              </div>
            </div>
          </div>
        </div>

        {/* Disk Card */}
        <div className="bg-background-light border border-slate-800 rounded-xl p-3 sm:p-4 flex flex-col justify-between shadow-lg">
          <h3 className="text-slate-200 text-[10px] sm:text-xs font-bold mb-2 sm:mb-3 uppercase tracking-wider">Disk Kullanımı</h3>
          <div className="space-y-3">
            <div>
              <div className="flex justify-between text-[10px] sm:text-xs mb-1">
                <span className="text-slate-400 truncate mr-2">Doluluk Oranı</span>
                <span className="text-emerald-400 font-bold shrink-0">%{diskUsagePercent.toFixed(1)}</span>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-1.5">
                <div className="bg-emerald-500 h-1.5 rounded-full transition-all duration-1000" style={{ width: `${Math.min(100, diskUsagePercent)}%` }}></div>
              </div>
              <div className="mt-2 text-[9px] sm:text-[10px] text-slate-500 flex justify-between gap-2">
                <span className="truncate">Boş: {freeDisk.toFixed(1)}G</span>
                <span className="truncate">Top: {totalDisk.toFixed(0)}G</span>
              </div>
            </div>
          </div>
          <div className="flex justify-between items-center mt-3 pt-2 border-t border-slate-800/50">
            <span className="text-[8px] sm:text-[9px] text-slate-500 font-bold tracking-wider">CANLI</span>
            <span className="text-[8px] sm:text-[10px] text-slate-400 truncate ml-2">Depolama</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Metrics;
