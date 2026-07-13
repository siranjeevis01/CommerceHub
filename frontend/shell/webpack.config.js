const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

const isProd = process.env.NODE_ENV === 'production';

module.exports = {
  output: {
    publicPath: 'auto',
    uniqueName: 'shell',
  },
  optimization: {
    runtimeChunk: false,
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'shell',
      remotes: {
        admin: `admin@${isProd ? '/admin/remoteEntry.js' : 'http://localhost:4201/remoteEntry.js'}`,
        vendor: `vendor@${isProd ? '/vendor/remoteEntry.js' : 'http://localhost:4202/remoteEntry.js'}`,
        storefront: `storefront@${isProd ? '/storefront/remoteEntry.js' : 'http://localhost:4203/remoteEntry.js'}`,
      },
      shared: {
        '@angular/core': { singleton: true, strictVersion: true },
        '@angular/common': { singleton: true, strictVersion: true },
        '@angular/router': { singleton: true, strictVersion: true },
        'rxjs': { singleton: true, strictVersion: true },
      },
    }),
  ],
};
