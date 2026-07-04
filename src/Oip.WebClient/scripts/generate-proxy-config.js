const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const projectRoot = path.resolve(__dirname, '..');
const outputDirectory = path.join(projectRoot, 'obj');
const outputFilePath = path.join(outputDirectory, 'proxy.generated.json');

const defaultTarget = process.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS ? process.env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';

function createKeepAliveProxy(context, target) {
  return {
    context,
    target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    },
    ws: true
  };
}

function createWsProxy(context, target) {
  return {
    context,
    target,
    secure: false,
    changeOrigin: true,
    ws: true
  };
}

function createProxyConfig(configFromApi) {
  if (configFromApi.standalone) {
    return [
      createWsProxy(
        [
          '/hubs/notification'
        ],
        configFromApi.targets.main
      ),
      createKeepAliveProxy(
        [
          '/manifest.json',
          '/api',
          '/signin-oidc',
          '/signout-callback-oidc',
          '/signout-oidc',
          '/swagger',
          '/health',
          '/metrics'
        ],
        configFromApi.targets.main
      )
    ];
  } else {
    return [
      createKeepAliveProxy(
        [
          '/api/users',
          '/api/user-profile'
        ],
        configFromApi.targets.users
      ),
      createWsProxy(
        [
          '/hubs/notification',
          '/api/notification'
        ],
        configFromApi.targets.notification
      ),
      createWsProxy(
        [
          '/api/discussion'
        ],
        configFromApi.targets.discussion
      ),
      createWsProxy(
        [
          '/api/applications'
        ],
        configFromApi.targets.applications
      ),
      createKeepAliveProxy(
        [
          '/manifest.json',
          '/api',
          '/signin-oidc',
          '/signout-callback-oidc',
          '/signout-oidc',
          '/swagger',
          '/health',
          '/metrics'
        ],
        configFromApi.targets.main
      )
    ];
  }
}

async function loadProxyConfigFromApi() {
  let lastError;

  for (let attempt = 1; attempt <= 10; attempt += 1) {
    try {
      console.log(`Loading proxy settings from ${defaultTarget}, attempt ${attempt}/10`);

      const response = await fetch(`${defaultTarget}/api/proxy-settings/get-spa-proxy-settings`);

      if (!response.ok) {
        throw new Error(`Proxy settings request failed with status ${response.status}`);
      }

      return await response.json()
    } catch (error) {
      lastError = error;
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
  }

  throw lastError ?? new Error('Failed to fetch proxy settings from API');
}

function writeConfig(config) {
  fs.mkdirSync(outputDirectory, {recursive: true});
  fs.writeFileSync(outputFilePath, JSON.stringify(config, null, 2));
}

async function main() {
  const targets = await loadProxyConfigFromApi();
  const proxyConfig = createProxyConfig(targets)
  writeConfig(proxyConfig);
  console.log(`Proxy config generated from API: ${outputFilePath}`);
}

main().catch(error => {
  console.error(error);
  process.exit(1);
});
