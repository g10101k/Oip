const { shareAll, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

module.exports = withModuleFederationPlugin({
  name: 'oipAngularModule',

  exposes: {
    './ExternalModuleExampleModule':
      './projects/oip/src/app/components/external-module-example-module/external-module-example-module.component.ts',
  },

  shared: {
    ...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto' }),
  }
});
