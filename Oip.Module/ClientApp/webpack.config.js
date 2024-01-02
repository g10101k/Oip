const {shareAll, withModuleFederationPlugin} = require('@angular-architects/module-federation/webpack');
const moduleFederation = require('../appsettings.json').ModuleFederation; //(with path)
const exportModule = moduleFederation.ExportModule;
const exposes = {};
exposes[exportModule.ExposedModule] = exportModule.SourcePath;

module.exports = withModuleFederationPlugin({
  name: moduleFederation.Name,
  exposes: exposes,
  shared: {
    ...shareAll({singleton: true, strictVersion: true, requiredVersion: 'auto'}),
  },
});
