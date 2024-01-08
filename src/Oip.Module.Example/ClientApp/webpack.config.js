const {shareAll, withModuleFederationPlugin} = require('@angular-architects/module-federation/webpack');
const moduleFederation = require('../appsettings.modules.json').ModuleFederation; //(with path)
const exposes = {};
moduleFederation.ExportModules.forEach(x =>   exposes[x.ExposedModule] = x.SourcePath);

module.exports = withModuleFederationPlugin({
  name: moduleFederation.Name,
  exposes: exposes,
  shared: {
    ...shareAll({singleton: true, strictVersion: true, requiredVersion: 'auto'}),
  },
});
