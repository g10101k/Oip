const { withNativeFederation, shareAll } = require('@angular-architects/native-federation/config');

const sharedConfig = { singleton: true, strictVersion: true, requiredVersion: 'auto' };

module.exports = withNativeFederation({
  name: 'oip',


  shared: {
    ...shareAll(sharedConfig),
  },

  skip: [
    'rxjs/ajax',
    'rxjs/fetch',
    'rxjs/testing',
    'rxjs/webSocket',
    '@primeng/themes',
    // Add further packages you don't need at runtime
  ],

  // Please read our FAQ about sharing libs:
  // https://shorturl.at/jmzH0

  features: {
    // New feature for more performance and avoiding
    // issues with node libs. Comment this out to
    // get the traditional behavior:
    ignoreUnusedDeps: true
  }
});
