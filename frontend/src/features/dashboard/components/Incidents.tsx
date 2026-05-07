import React, { useState, useEffect } from 'react';
import { Loader2, CheckCircle2, Clock, ShieldAlert, X, Maximize2 } from 'lucide-react';
import { dashboardService } from '../../../api/dashboardService';
import { useAuth } from '../../../context/AuthContext';
import { useSignalR } from '../../../context/SignalRContext';
import type { IncidentDto } from '../../../types/dashboard.types';

type TabType = 'active' | 'resolved';

interface IncidentsProps {
  appId?: string;
  readOnly?: boolean;
}

const Incidents: React.FC<IncidentsProps> = ({ appId, readOnly = false }) => {
  const { token } = useAuth();
  const [incidents, setIncidents] = useState<IncidentDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<TabType>('active');
  const [selectedIncident, setSelectedIncident] = useState<IncidentDto | null>(null);

  // JSON Parser Helper: Sadece Unhealthy olanları ayıklar
  const parseUnhealthyComponents = (errorMessage: string) => {
    try {
      if (!errorMessage.trim().startsWith('{')) return null;
      const data = JSON.parse(errorMessage);
      const unhealthy: { name: string, description: string }[] = [];

      Object.entries(data).forEach(([key, value]: [string, any]) => {
        const status = (typeof value === 'string' ? value : value.status)?.toString() || '';
        if (status.includes('Unhealthy') || status === '3') {
          unhealthy.push({
            name: key,
            description: value.description || 'Hata detayı belirtilmedi.'
          });
        }
      });
      return unhealthy.length > 0 ? unhealthy : null;
    } catch {
      return null;
    }
  };

  const formatJson = (json: string) => {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  };

  const { connection, isConnected } = useSignalR();

  useEffect(() => {
    // Sayfa açıldığında tüm yetkili hataları çek (selectedAppId değişimine bağımlı değil)
    fetchIncidents();

    if (!connection || !isConnected) return;

    const handleNewIncident = (newIncident: IncidentDto) => {
      // Filtreleme: Eğer appId gelmişse sadece o uygulamaya ait olayları ekle
      if (appId && newIncident.appId !== appId) return;

      setIncidents(prev => {
        if (prev.some(i => i.id === newIncident.id)) return prev;
        return [newIncident, ...prev];
      });
    };

    const handleResolvedIncident = (resolvedIncident: IncidentDto) => {
      setIncidents(prev => prev.map(i => i.id === resolvedIncident.id ? resolvedIncident : i));
    };

    connection.on('ReceiveNewIncident', handleNewIncident);
    connection.on('ReceiveResolvedIncident', handleResolvedIncident);

    return () => {
      connection.off('ReceiveNewIncident', handleNewIncident);
      connection.off('ReceiveResolvedIncident', handleResolvedIncident);
    };
  }, [connection, isConnected, appId]);

  const fetchIncidents = async () => {
    try {
      setLoading(true);
      // appId varsa filtreleyerek çek, yoksa global çek (DashboardView / GlobalDashboard ayrımı)
      const data = await dashboardService.getIncidents(appId);
      // Sıralama: En yeniden eskiye
      const sorted = data.sort((a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime());
      setIncidents(sorted);
    } catch (err) {
      console.error('Failed to fetch incidents', err);
    } finally {
      setLoading(false);
    }
  };

  const activeIncidents = incidents.filter(i => !i.resolvedAt);
  const resolvedIncidents = incidents.filter(i => !!i.resolvedAt);

  const displayList = activeTab === 'active' ? activeIncidents : resolvedIncidents;

  const handleResolve = async (id: string) => {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/Incidents/${id}/resolve`, {
        method: 'PATCH',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        // State güncellenmesi SignalR üzerinden de gelecek ama hızlı tepki için burada da yapabiliriz
        setIncidents(prev => prev.map(i => i.id === id ? { ...i, resolvedAt: new Date().toISOString() } : i));
      }
    } catch (error) {
      console.error('Hata çözülürken bir sorun oluştu:', error);
    }
  };

  return (
    <div className="bg-white/5 border border-white/10 rounded-2xl shadow-xl flex flex-col overflow-hidden h-full">
      <div className="flex flex-col bg-white/5">
      {/* Header (Aligned with Metrics and AI Tower) */}
      <div className="h-[52px] px-4 border-b border-white/10 flex items-center justify-between bg-white/5 shrink-0">
        <div className="flex items-center gap-3">
          <ShieldAlert size={18} className="text-rose-500" />
          <h2 className="text-sm font-black text-slate-100 uppercase tracking-[0.2em]">Sistem Uyarıları</h2>
        </div>
          <div className="px-3 py-1 bg-rose-500/10 border border-rose-500/20 text-rose-500 text-[10px] font-black uppercase tracking-widest rounded-full">
            {activeIncidents.length} Açık Uyarı
          </div>
        </div>
        <div className="flex px-4 pt-1">
          <button 
            onClick={() => setActiveTab('active')}
            className={`px-4 py-3 text-[10px] font-black uppercase tracking-widest border-b-2 transition-all ${activeTab === 'active' ? 'border-rose-500 text-rose-400' : 'border-transparent text-slate-500 hover:text-slate-300'}`}
          >
            Aktif Hatalar
          </button>
          <button 
            onClick={() => setActiveTab('resolved')}
            className={`px-4 py-3 text-[10px] font-black uppercase tracking-widest border-b-2 transition-all ${activeTab === 'resolved' ? 'border-emerald-500 text-emerald-400' : 'border-transparent text-slate-500 hover:text-slate-300'}`}
          >
            Çözülen Hatalar
          </button>
        </div>
      </div>

      {/* Incident List */}
      <div className="flex flex-col flex-1 overflow-y-auto p-3.5 gap-3 h-full custom-scrollbar">
        {loading ? (
          <div className="flex items-center justify-center py-10 text-slate-500">
            <Loader2 className="w-6 h-6 animate-spin mr-2" />
            Yükleniyor...
          </div>
        ) : displayList.length === 0 ? (
          <div className="flex items-center justify-center py-10 text-slate-500 text-sm">
            Bu sekmede gösterilecek kayıt bulunamadı.
          </div>
        ) : (
          displayList.map((incident) => {
            const unhealthyComponents = parseUnhealthyComponents(incident.errorMessage);
            
            return (
              <div 
                key={incident.id} 
                className={`p-4 rounded-xl border transition-all ${
                  activeTab === 'active' 
                    ? 'bg-rose-500/[0.03] border-rose-500/20 hover:border-rose-500/40' 
                    : 'bg-white/[0.02] border-white/5 grayscale opacity-70'
                }`}
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-xl ${activeTab === 'active' ? 'bg-rose-500/10 text-rose-500 border border-rose-500/20' : 'bg-white/5 text-slate-500 border border-white/5'}`}>
                      <ShieldAlert size={18} />
                    </div>
                    <div>
                      <h4 className="text-sm font-bold text-slate-200">{incident.appName}</h4>
                      <div className="flex items-center gap-2 text-[10px] text-slate-500 font-bold uppercase tracking-wider mt-1">
                        <span className="px-1.5 py-0.5 bg-black/20 rounded border border-white/5 text-indigo-400">
                          {incident.failedComponent || 'System'}
                        </span>
                        <div className="flex items-center gap-1 opacity-60">
                          <Clock size={10} />
                          <span>{new Date(incident.startedAt).toLocaleString('tr-TR')}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                  {activeTab === 'resolved' && (
                    <div className="flex items-center gap-1 text-emerald-500 text-[10px] font-black uppercase tracking-widest bg-emerald-500/10 border border-emerald-500/20 px-2 py-1 rounded-lg">
                      <CheckCircle2 size={10} />
                      <span>Çözüldü</span>
                    </div>
                  )}
                </div>

                {/* Bileşen Bazlı Hata Listesi */}
                <div className="space-y-2 mb-4">
                  {unhealthyComponents ? (
                    unhealthyComponents.map((comp, idx) => (
                      <div key={idx} className="flex items-start gap-2 text-xs bg-black/20 p-2.5 rounded-xl border border-white/5">
                        <div className="w-1.5 h-1.5 rounded-full bg-rose-500 mt-1.5 shrink-0 shadow-[0_0_8px_rgba(244,63,94,0.5)]" />
                        <div>
                          <span className="font-black text-rose-400 uppercase tracking-tight text-[10px]">{comp.name}:</span>
                          <p className="text-slate-400 text-[11px] font-medium mt-0.5 leading-relaxed">{comp.description}</p>
                        </div>
                      </div>
                    ))
                  ) : (
                    <p className="text-xs text-slate-300 bg-rose-500/10 p-2.5 rounded-xl border border-rose-500/10 italic">
                      {incident.errorMessage}
                    </p>
                  )}
                </div>

                <div className="flex gap-2">
                  <button 
                    onClick={() => setSelectedIncident(incident)}
                    className="flex-1 flex items-center justify-center gap-2 py-2 text-[10px] font-black uppercase tracking-widest text-slate-500 hover:text-indigo-400 bg-white/5 hover:bg-indigo-500/10 rounded-xl border border-white/5 hover:border-indigo-500/30 transition-all"
                  >
                    <Maximize2 size={12} />
                    Detaylar
                  </button>
                  
                  {activeTab === 'active' && !readOnly && (
                    <button 
                      onClick={() => handleResolve(incident.id)}
                      className="flex-1 flex items-center justify-center gap-2 py-2 text-[10px] font-black uppercase tracking-widest text-emerald-500 hover:text-white bg-emerald-500/10 hover:bg-emerald-500 rounded-xl border border-emerald-500/20 transition-all active:scale-95"
                    >
                      <CheckCircle2 size={12} />
                      Çözüldü Yap
                    </button>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Incident Detail Modal */}
      {selectedIncident && (
        <div className="fixed inset-0 z-[120] flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => setSelectedIncident(null)} />
          <div className="relative w-full max-w-3xl bg-background-light border border-slate-800 rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh] animate-in fade-in zoom-in duration-200">
            <div className="p-6 border-b border-slate-800 flex items-center justify-between bg-white/5">
              <div className="flex items-center gap-4">
                <div className="p-3 bg-rose-500/10 text-rose-500 rounded-xl border border-rose-500/20">
                  <ShieldAlert size={24} />
                </div>
                <div>
                  <h3 className="text-lg font-black text-white uppercase tracking-[0.2em]">Olay Detayları</h3>
                  <p className="text-[10px] text-slate-500 font-bold uppercase tracking-widest mt-1">{selectedIncident.appName} - {selectedIncident.failedComponent || 'System'}</p>
                </div>
              </div>
              <button 
                onClick={() => setSelectedIncident(null)}
                className="p-2 text-slate-500 hover:text-white hover:bg-white/5 rounded-full transition-all"
              >
                <X size={24} />
              </button>
            </div>
            
            <div className="p-8 overflow-y-auto custom-scrollbar flex-1">
              <div className="grid grid-cols-2 gap-6 mb-8">
                <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                  <span className="text-[10px] text-slate-500 uppercase font-black tracking-widest">Başlangıç Zamanı</span>
                  <p className="text-slate-200 text-sm font-bold mt-1.5">{new Date(selectedIncident.startedAt).toLocaleString('tr-TR')}</p>
                </div>
                <div className="p-4 bg-white/5 rounded-2xl border border-white/5">
                  <span className="text-[10px] text-slate-500 uppercase font-black tracking-widest">Güncel Durum</span>
                  <p className={`mt-1.5 text-sm font-black uppercase tracking-widest ${selectedIncident.resolvedAt ? 'text-emerald-500' : 'text-rose-500 animate-pulse'}`}>
                    {selectedIncident.resolvedAt ? 'Çözüldü' : 'Devam Ediyor'}
                  </p>
                </div>
              </div>

              <div className="space-y-4">
                <h4 className="text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1">Hata Kaydı (Raw Log)</h4>
                <div className="relative group">
                  <pre className="p-6 bg-black/40 text-emerald-400 font-mono text-xs rounded-2xl border border-white/5 overflow-x-auto shadow-inner leading-relaxed">
                    {formatJson(selectedIncident.errorMessage)}
                  </pre>
                </div>
              </div>
            </div>

            <div className="p-6 bg-white/5 border-t border-white/10 flex justify-end">
              <button 
                onClick={() => setSelectedIncident(null)}
                className="px-8 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white text-[10px] font-black uppercase tracking-widest rounded-xl transition-all shadow-lg shadow-indigo-600/20 active:scale-95"
              >
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Incidents;
