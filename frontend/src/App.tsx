import AppRouter from './routes/AppRouter';
import { AuthProvider } from './context/AuthContext';
import { Toaster } from 'sonner';

function App() {
  return (
    <AuthProvider>
      <Toaster position="top-right" richColors theme="dark" />
      <AppRouter />
    </AuthProvider>
  );
}

export default App;
