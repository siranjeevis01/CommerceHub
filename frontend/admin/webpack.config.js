const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

module.exports = {
  output: {
    publicPath: 'auto',
    uniqueName: 'admin',
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'admin',
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/admin.module.ts'),
      },
      shared: {
        '@angular/core': { singleton: true },
        '@angular/common': { singleton: true },
        '@angular/router': { singleton: true },
      },
    }),
  ],
};
