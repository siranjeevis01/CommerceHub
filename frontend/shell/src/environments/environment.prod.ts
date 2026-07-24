export const environment = {
  production: true,
  apiUrl: (window as any).env?.API_URL || 'https://commercehub-api-5xg3.onrender.com',
  signalrUrl: (window as any).env?.SIGNALR_URL || 'https://commercehub-api-5xg3.onrender.com/hubs/notification',
  appName: 'CommerceHub',
  version: '1.0.0'
};
