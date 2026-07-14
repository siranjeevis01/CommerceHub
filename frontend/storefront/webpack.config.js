const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

const isProd = process.env.NODE_ENV === 'production' || process.env.NODE_ENV === 'staging' || (!process.argv.some(a => a.includes('serve')));

module.exports = {
  output: {
    publicPath: isProd ? '/storefront/' : 'auto',
    uniqueName: 'storefront',
    library: { name: 'storefront', type: 'var' },
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'storefront',
      filename: 'remoteEntry.js',
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/storefront.module.ts'),
      },
      shared: {
        '@angular/core': { singleton: true },
        '@angular/common': { singleton: true },
        '@angular/router': { singleton: true },
        '@angular/forms': { singleton: true },
        'rxjs': { singleton: true },
      },
    }),
  ],
};
