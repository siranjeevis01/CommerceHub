// Runtime environment injection for production
// Place environment variables in nginx envsubst or pass via Docker
(function() {
  window.env = window.env || {};
  window.env.API_URL = '${API_URL}';
  window.env.UPI_ID = '${UPI_ID}';
  window.env.FIREBASE_API_KEY = '${FIREBASE_API_KEY}';
})();
