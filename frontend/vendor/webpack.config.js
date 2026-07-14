const { ModuleFederationPlugin } = require('webpack').container;
const path = require('path');

const isProd = process.env.NODE_ENV === 'production' || process.env.NODE_ENV === 'staging' || (!process.argv.some(a => a.includes('serve')));

module.exports = {
  output: {
    publicPath: isProd ? '/vendor/' : 'auto',
    uniqueName: 'vendor',
  },
  optimization: {
    runtimeChunk: false,
  },
  plugins: [
    new ModuleFederationPlugin({
      name: 'vendor',
      filename: 'remoteEntry.js',
      library: { type: 'var', name: 'vendor' },
      exposes: {
        './Module': path.resolve(__dirname, 'src/app/vendor.module.ts'),
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
