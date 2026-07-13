const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

module.exports = {
  output: {
    publicPath: 'auto',
    uniqueName: 'storefront',
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'storefront',
      filename: 'remoteEntry.js',
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/storefront.module.ts'),
      },
      shared: {
        '@angular/core': { singleton: true, requiredVersion: 'auto' },
        '@angular/common': { singleton: true, requiredVersion: 'auto' },
        '@angular/router': { singleton: true, requiredVersion: 'auto' },
        '@angular/forms': { singleton: true, requiredVersion: 'auto' },
        'rxjs': { singleton: true, requiredVersion: 'auto' },
      },
    }),
  ],
};
