import React, { useState, useEffect } from 'react';
import { AlertTriangle, Loader2, CheckCircle2, Clock, ShieldAlert, X, Maximize2 } from 'lucide-react';
import { dashboardService } from '../api/dashboardService';
import { useAuth } from '../../../context/AuthContext';
import { useSignalR } from '../../../context/SignalRContext';
import type { IncidentDto } from '../../../types/dashboard.types';

type TabType = 'active' | 'resolved';

interface IncidentsProps {
  selectedAppId?: string;
}

const Incidents: React.FC<IncidentsProps> = ({ selectedAppId }) => {
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
    if (selectedAppId) {
      fetchIncidents();
    }

    if (!connection || !isConnected) return;

    const handleNewIncident = (newIncident: IncidentDto) => {
      // Sadece seçili uygulama için veya tümü seçiliyse ekle
      if (!selectedAppId || newIncident.appId === selectedAppId) {
        setIncidents(prev => {
          if (prev.some(i => i.id === newIncident.id)) return prev;
          return [newIncident, ...prev];
        });
      }
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
  }, [selectedAppId, connection, isConnected]);

  const fetchIncidents = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getIncidents(selectedAppId);
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
    <div className="bg-background-light border border-slate-800 rounded-xl shadow-lg flex flex-col overflow-hidden min-h-[300px]">
      {/* ... (Header & Tabs aynı kalıyor) */}
      <div className="flex flex-col border-b border-slate-800 bg-slate-800/20">
        <div className="h-14 px-5 flex items-center justify-between border-b border-slate-800/50">
          <div className="flex items-center gap-2">
            <AlertTriangle size={18} className="text-rose-500" />
            <h3 className="text-slate-100 font-semibold">Sistem Uyarıları</h3>
          </div>
          <div className="px-3 py-1 bg-rose-500/10 border border-rose-500/20 text-rose-500 text-xs font-medium rounded-full">
            {activeIncidents.length} Açık Uyarı
          </div>
        </div>
        <div className="flex px-5 pt-2">
          <button 
            onClick={() => setActiveTab('active')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${activeTab === 'active' ? 'border-rose-500 text-rose-400' : 'border-transparent text-slate-400 hover:text-slate-300'}`}
          >
            Aktif Hatalar
          </button>
          <button 
            onClick={() => setActiveTab('resolved')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${activeTab === 'resolved' ? 'border-emerald-500 text-emerald-400' : 'border-transparent text-slate-400 hover:text-slate-300'}`}
          >
            Çözülen Hatalar
          </button>
        </div>
      </div>

      {/* Incident List */}
      <div className="flex flex-col flex-1 overflow-y-auto p-4 gap-4 max-h-[550px] custom-scrollbar">
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
                className={`p-4 rounded-lg border transition-all ${
                  activeTab === 'active' 
                    ? 'bg-rose-500/5 border-rose-500/20 hover:border-rose-500/40' 
                    : 'bg-slate-800/20 border-slate-700/50 grayscale opacity-70'
                }`}
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-lg ${activeTab === 'active' ? 'bg-rose-500/20 text-rose-500' : 'bg-slate-700 text-slate-400'}`}>
                      <ShieldAlert size={20} />
                    </div>
                    <div>
                      <h4 className="font-semibold text-slate-100">{incident.appName}</h4>
                      <div className="flex items-center gap-2 text-xs text-slate-400 mt-1">
                        <span className="px-1.5 py-0.5 bg-slate-800 rounded border border-slate-700">
                          {incident.failedComponent || 'System'}
                        </span>
                        <Clock size={12} />
                        <span>{new Date(incident.startedAt).toLocaleString('tr-TR')}</span>
                      </div>
                    </div>
                  </div>
                  {activeTab === 'resolved' && (
                    <div className="flex items-center gap-1 text-emerald-500 text-xs font-medium bg-emerald-500/10 px-2 py-1 rounded">
                      <CheckCircle2 size={12} />
                      <span>Çözüldü</span>
                    </div>
                  )}
                </div>

                {/* Bileşen Bazlı Hata Listesi */}
                <div className="space-y-2 mb-4">
                  {unhealthyComponents ? (
                    unhealthyComponents.map((comp, idx) => (
                      <div key={idx} className="flex items-start gap-2 text-sm bg-background/50 p-2 rounded border border-slate-800">
                        <span className="text-rose-500 mt-1">🔴</span>
                        <div>
                          <span className="font-bold text-rose-400">{comp.name}:</span>
                          <p className="text-slate-400 text-xs mt-0.5">{comp.description}</p>
                        </div>
                      </div>
                    ))
                  ) : (
                    <p className="text-sm text-slate-300 bg-rose-500/10 p-2 rounded italic">
                      {incident.errorMessage}
                    </p>
                  )}
                </div>

                <div className="flex gap-2">
                  <button 
                    onClick={() => setSelectedIncident(incident)}
                    className="flex-1 flex items-center justify-center gap-2 py-2 text-xs font-medium text-slate-400 hover:text-indigo-400 bg-slate-800/50 hover:bg-indigo-500/10 rounded border border-slate-700 hover:border-indigo-500/30 transition-all"
                  >
                    <Maximize2 size={14} />
                    Detaylar
                  </button>
                  
                  {activeTab === 'active' && (
                    <button 
                      onClick={() => handleResolve(incident.id)}
                      className="flex-1 flex items-center justify-center gap-2 py-2 text-xs font-medium text-emerald-400 hover:text-emerald-300 bg-emerald-500/10 hover:bg-emerald-500/20 rounded border border-emerald-500/20 hover:border-emerald-500/40 transition-all"
                    >
                      <CheckCircle2 size={14} />
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
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-background/80 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="w-full max-w-3xl bg-background-light border border-slate-800 rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
            <div className="p-6 border-b border-slate-800 flex items-center justify-between bg-slate-800/30">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-rose-500/20 text-rose-500 rounded-lg">
                  <ShieldAlert size={24} />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-slate-100">Olay Detayları</h3>
                  <p className="text-sm text-slate-400">{selectedIncident.appName} - {selectedIncident.failedComponent || 'System'}</p>
                </div>
              </div>
              <button 
                onClick={() => setSelectedIncident(null)}
                className="p-2 text-slate-400 hover:text-slate-100 hover:bg-slate-800 rounded-full transition-colors"
              >
                <X size={24} />
              </button>
            </div>
            
            <div className="p-6 overflow-y-auto custom-scrollbar flex-1 bg-background/50">
              <div className="grid grid-cols-2 gap-4 mb-6">
                <div className="p-4 bg-slate-800/30 rounded-xl border border-slate-700">
                  <span className="text-xs text-slate-500 uppercase font-bold tracking-wider">Başlangıç</span>
                  <p className="text-slate-200 mt-1">{new Date(selectedIncident.startedAt).toLocaleString('tr-TR')}</p>
                </div>
                <div className="p-4 bg-slate-800/30 rounded-xl border border-slate-700">
                  <span className="text-xs text-slate-500 uppercase font-bold tracking-wider">Durum</span>
                  <p className={`mt-1 font-bold ${selectedIncident.resolvedAt ? 'text-emerald-500' : 'text-rose-500 animate-pulse'}`}>
                    {selectedIncident.resolvedAt ? 'Çözüldü' : 'Devam Ediyor'}
                  </p>
                </div>
              </div>

              <div className="space-y-4">
                <h4 className="text-sm font-bold text-slate-400 uppercase tracking-wider">Ham Log Verisi</h4>
                <div className="relative group">
                  <pre className="p-5 bg-slate-950 text-emerald-400 font-mono text-sm rounded-xl border border-slate-800 overflow-x-auto shadow-inner leading-relaxed">
                    {formatJson(selectedIncident.errorMessage)}
                  </pre>
                </div>
              </div>
            </div>

            <div className="p-6 bg-slate-800/30 border-t border-slate-800 flex justify-end">
              <button 
                onClick={() => setSelectedIncident(null)}
                className="px-6 py-2 bg-indigo-600 hover:bg-indigo-500 text-white font-bold rounded-lg transition-all shadow-lg shadow-indigo-600/20 active:scale-95"
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
