import React, { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (context === undefined) {
    throw new Error('useSignalR must be used within a SignalRProvider');
  }
  return context;
};

interface SignalRProviderProps {
  children: ReactNode;
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ children }) => {
  const { token } = useAuth();
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    // Token yoksa bağlantı kurma (Hub [Authorize] beklediği için)
    if (!token) {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
        setIsConnected(false);
      }
      return;
    }

    // Eğer zaten bir bağlantı varsa ve token değiştiyse eskisini kapat
    if (connectionRef.current) {
        connectionRef.current.stop();
    }

    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5226';
    
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/statushub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.None)
      .build();

    const startConnection = async () => {
      try {
        await newConnection.start();
        console.log('>>>> [SIGNALR] Merkezi Hub Bağlantısı Başarılı.');
        setIsConnected(true);
      } catch (err: any) {
        const errorMsg = err?.toString() || "";
        const errorName = err?.name || "";
        // React Strict Mode remount hatalarını sustur
        if (errorMsg.includes('AbortError') || errorName.includes('AbortError') || errorMsg.includes('stopped')) return;
        
        console.error('>>>> [SIGNALR] Merkezi Hub Bağlantı Hatası:', err);
        setIsConnected(false);
      }
    };

    newConnection.onreconnecting(() => setIsConnected(false));
    newConnection.onreconnected(() => setIsConnected(true));
    newConnection.onclose(() => setIsConnected(false));

    startConnection();
    connectionRef.current = newConnection;

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [token]);

  return (
    <SignalRContext.Provider value={{ connection: connectionRef.current, isConnected }}>
      {children}
    </SignalRContext.Provider>
  );
};
