import AppRouter from './routes/AppRouter';
import { AuthProvider } from './context/AuthContext';
import { SignalRProvider } from './context/SignalRContext';
import { Toaster } from 'sonner';

function App() {
  return (
    <AuthProvider>
      <SignalRProvider>
        <Toaster position="top-right" richColors theme="dark" />
        <AppRouter />
      </SignalRProvider>
    </AuthProvider>
  );
}

export default App;
