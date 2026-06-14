const { createBuilder } = require('@angular-devkit/architect');
const { buildWebpackBrowser } = require('@angular-devkit/build-angular/src/builders/browser');
const { container } = require('webpack');
const path = require('node:path');

const { ModuleFederationPlugin } = container;

module.exports = createBuilder((options, context) =>
  buildWebpackBrowser(options, context, {
    webpackConfiguration: async (config) => {
      const workspaceRoot = context.workspaceRoot;
      const packageJson = require(path.join(workspaceRoot, 'package.json'));
      const dependencies = packageJson.dependencies ?? {};

      config.output = {
        ...config.output,
        publicPath: 'auto',
        uniqueName: 'oip'
      };

      config.optimization = {
        ...config.optimization,
        runtimeChunk: false
      };

      config.plugins = [
        ...(config.plugins ?? []),
        new ModuleFederationPlugin({
          name: 'oip',
          filename: 'remoteEntry.js',
          exposes: {
            './Routes': './projects/oip/src/remote.routes.ts'
          },
          shared: {
            '@angular/animations': sharedDependency(dependencies, '@angular/animations', true),
            '@angular/common': sharedDependency(dependencies, '@angular/common', true),
            '@angular/core': sharedDependency(dependencies, '@angular/core', true),
            '@angular/forms': sharedDependency(dependencies, '@angular/forms', true),
            '@angular/router': sharedDependency(dependencies, '@angular/router', true),
            '@ngx-translate/core': sharedDependency(dependencies, '@ngx-translate/core', true),
            '@primeng/themes': sharedDependency(dependencies, '@primeng/themes', true),
            'oip-common': sharedDependency(dependencies, 'oip-common', true),
            primeng: sharedDependency(dependencies, 'primeng', true),
            rxjs: sharedDependency(dependencies, 'rxjs', false)
          }
        })
      ];

      return config;
    }
  })
);

function sharedDependency(dependencies, packageName, strictVersion) {
  return {
    singleton: true,
    strictVersion,
    requiredVersion: dependencies[packageName] ?? false
  };
}

