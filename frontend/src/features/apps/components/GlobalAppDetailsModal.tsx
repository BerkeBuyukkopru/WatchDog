import React from 'react';
import { X, Globe } from 'lucide-react';
import Incidents from '../../dashboard/components/Incidents';
import { AiTower } from '../../ai-tower/components/AiTower';

interface GlobalAppDetailsModalProps {
  appId: string;
  appName: string;
  onClose: () => void;
}

const GlobalAppDetailsModal: React.FC<GlobalAppDetailsModalProps> = ({ appId, appName, onClose }) => {
  return (
    <div className="fixed top-0 left-0 w-full h-full z-[9999] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-background/80 backdrop-blur-md animate-in fade-in duration-300" 
        onClick={onClose}
      />

      {/* Modal Box */}
      <div className="relative z-10 w-full max-w-[95vw] h-[90vh] bg-background-light border border-slate-800 rounded-3xl shadow-2xl overflow-hidden flex flex-col animate-in zoom-in-95 duration-300">
        
        {/* Header Section */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-slate-900/50">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center text-indigo-400">
              <Globe size={24} />
            </div>
            <div>
              <h3 className="text-xl font-black text-slate-100 uppercase tracking-widest">{appName}</h3>
              <p className="text-xs text-slate-500 font-bold uppercase tracking-widest flex items-center gap-2">
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                Uygulama Analiz Merkezi
              </p>
            </div>
          </div>
          
          <button 
            onClick={onClose}
            className="p-3 text-slate-400 hover:text-slate-100 hover:bg-slate-800 rounded-2xl transition-all"
          >
            <X size={28} />
          </button>
        </div>

        {/* Content Section: 2 Columns on Desktop, Stacked on Mobile */}
        <div className="flex-1 flex flex-col lg:flex-row overflow-y-auto lg:overflow-hidden">
          
          {/* Left Column: Incidents */}
          <div className="flex-1 border-b lg:border-b-0 lg:border-r border-slate-800 flex flex-col min-w-0 min-h-[400px] lg:min-h-0">
            <div className="flex-1 overflow-hidden lg:overflow-hidden">
              <Incidents appId={appId} readOnly={true} />
            </div>
          </div>

          {/* Right Column: AI Insights */}
          <div className="flex-1 flex flex-col min-w-0 min-h-[400px] lg:min-h-0">
            <div className="flex-1 overflow-hidden lg:overflow-hidden">
              <AiTower selectedAppId={appId} readOnly={true} />
            </div>
          </div>

        </div>

        {/* Footer */}
        <div className="p-4 border-t border-slate-800 bg-slate-900/50 flex justify-end">
          <button 
            onClick={onClose}
            className="px-8 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 font-black text-xs uppercase tracking-widest rounded-xl transition-all border border-slate-700"
          >
            Kapat
          </button>
        </div>
      </div>
    </div>
  );
};

export default GlobalAppDetailsModal;
