const { shareAll, withModuleFederationPlugin } = require("@angular-architects/module-federation/webpack");

module.exports = withModuleFederationPlugin({
  shared: {
    ...shareAll({ singleton: true, strictVersion: true, requiredVersion: "auto" }),
    // oip-common is built from this workspace, so its version is not in package.json
    "oip-common": { singleton: true, strictVersion: false, requiredVersion: false }
  }
});
