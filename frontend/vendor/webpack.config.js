const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

module.exports = {
  output: {
    publicPath: 'http://localhost:4202/',
    uniqueName: 'vendor',
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'vendor',
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/vendor.module.ts'),
      },
      shared: {
        '@angular/core': { singleton: true },
        '@angular/common': { singleton: true },
        '@angular/router': { singleton: true },
      },
    }),
  ],
};
