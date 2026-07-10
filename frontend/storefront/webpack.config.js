const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

module.exports = {
  output: {
    publicPath: 'http://localhost:4203/',
    uniqueName: 'storefront',
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'storefront',
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/storefront.module.ts'),
      },
      shared: {
        '@angular/core': { singleton: true },
        '@angular/common': { singleton: true },
        '@angular/router': { singleton: true },
      },
    }),
  ],
};
