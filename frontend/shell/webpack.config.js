const { share, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

const isProd = process.env.NODE_ENV === 'production' || process.env.NODE_ENV === 'staging' || (!process.argv.some(a => a.includes('serve')));

module.exports = withModuleFederationPlugin({
  remotes: isProd ? {
    admin: 'admin/remoteEntry.js',
    vendor: 'vendor/remoteEntry.js',
    storefront: 'storefront/remoteEntry.js',
  } : {
    admin: 'http://localhost:4201/remoteEntry.js',
    vendor: 'http://localhost:4202/remoteEntry.js',
    storefront: 'http://localhost:4203/remoteEntry.js',
  },
  shared: share({
    '@angular/core': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    '@angular/common': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    '@angular/common/http': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    '@angular/router': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    '@angular/forms': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    '@angular/animations': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
    'rxjs': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
  }),
});
