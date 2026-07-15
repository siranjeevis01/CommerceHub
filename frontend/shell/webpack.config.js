const { shareAll, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

const isProd = process.env.NODE_ENV === 'production' || process.env.NODE_ENV === 'staging' || (!process.argv.some(a => a.includes('serve')));

module.exports = withModuleFederationPlugin({
  remotes: isProd ? {
    admin: 'admin@/admin/remoteEntry.js',
    vendor: 'vendor@/vendor/remoteEntry.js',
    storefront: 'storefront@/storefront/remoteEntry.js',
  } : {
    admin: 'admin@http://localhost:4201/remoteEntry.js',
    vendor: 'vendor@http://localhost:4202/remoteEntry.js',
    storefront: 'storefront@http://localhost:4203/remoteEntry.js',
  },
  shared: {
    ...shareAll({
      singleton: true,
      strictVersion: true,
      requiredVersion: 'auto',
    }),
  },
});
