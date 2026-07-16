export const environment = {
  production: true,
  apiUrl: (window as any).env?.API_URL || 'https://commercehub-api-5xg3.onrender.com',
  appName: 'CommerceHub Store',
  appVersion: '1.0.0',
  upiId: (window as any).env?.UPI_ID || 'commercehub@upi',
};
