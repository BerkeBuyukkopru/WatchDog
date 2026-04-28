/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        background: {
          DEFAULT: '#0f172a', // slate-900
          darker: '#020617', // slate-950
        },
        primary: {
          DEFAULT: '#6366f1', // indigo-500
        },
        accent: {
          DEFAULT: '#22d3ee', // cyan-400
        },
        status: {
          healthy: '#10b981', // emerald-500
          degraded: '#f59e0b', // amber-500
          unhealthy: '#f43f5e', // rose-500
        }
      }
    },
  },
  plugins: [],
}
