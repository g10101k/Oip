const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const projectRoot = path.resolve(__dirname, '..');
const outputDirectory = path.join(projectRoot, 'obj');
const outputFilePath = path.join(outputDirectory, 'proxy.generated.json');

const defaultTarget = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
    ? process.env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:5002';

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

function createProxyConfig(targets, appMode) {
  const normalizedTargets = {
    main: targets.main || defaultTarget,
    users: targets.users || process.env.OIP_USERS_TARGET || 'https://localhost:5005',
    discussion: targets.discussion || process.env.OIP_DISCUSSION_TARGET || 'https://localhost:5006',
    notification: targets.notification || process.env.OIP_NOTIFICATION_TARGET || 'https://localhost:5007'
  };

  const standaloneProxy = [
    createWsProxy(
      [
        '/hubs/notification'
      ],
      normalizedTargets.main
    ),
    createKeepAliveProxy(
      [
        '/api',
        '/swagger',
        '/health',
        '/metrics'
      ],
      normalizedTargets.main
    )
  ];

  const distributedProxy = [
    createKeepAliveProxy(
      [
        '/api/users',
        '/api/user-profile'
      ],
      normalizedTargets.users
    ),
    createWsProxy(
      [
        '/hubs/notification'
      ],
      normalizedTargets.notification
    ),
    createWsProxy(
      [
        '/api/discussion'
      ],
      normalizedTargets.discussion
    ),
    createKeepAliveProxy(
      [
        '/api',
        '/swagger',
        '/health',
        '/metrics'
      ],
      normalizedTargets.main
    )
  ];

  return appMode === 'distributed' ? distributedProxy : standaloneProxy;
}

function getAppMode(targets) {
  if (process.env.OIP_APP_MODE) {
    return process.env.OIP_APP_MODE === 'distributed' ? 'distributed' : 'standalone';
  }

  return targets.main === targets.users
  && targets.main === targets.discussion
  && targets.main === targets.notification
    ? 'standalone'
    : 'distributed';
}

function getTargetsFromApiResponse(apiResponse) {
  const targets = apiResponse?.targets || apiResponse?.Targets || {};

  return {
    main: targets.main || targets.Main || defaultTarget,
    users: targets.users || targets.Users,
    discussion: targets.discussion || targets.Discussion,
    notification: targets.notification || targets.Notification
  };
}

async function loadTargetsFromApi() {
  let lastError;

  for (let attempt = 1; attempt <= 10; attempt += 1) {
    try {
      console.log(`Loading proxy settings from ${defaultTarget}, attempt ${attempt}/10`);

      const response = await fetch(`${defaultTarget}/api/proxy-settings/get-spa-proxy-settings`);

      if (!response.ok) {
        throw new Error(`Proxy settings request failed with status ${response.status}`);
      }

      return getTargetsFromApiResponse(await response.json());
    } catch (error) {
      lastError = error;
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
  }

  throw lastError ?? new Error('Failed to fetch proxy settings from API');
}

function writeConfig(config) {
  fs.mkdirSync(outputDirectory, { recursive: true });
  fs.writeFileSync(outputFilePath, JSON.stringify(config, null, 2));
}

async function main() {
  try {
    const targets = await loadTargetsFromApi();
    const appMode = getAppMode(targets);
    const proxyConfig = createProxyConfig(targets, appMode);

    writeConfig(proxyConfig);
    console.log(`Proxy config generated from API: ${outputFilePath}`);
  } catch (error) {
    const fallbackMode = process.env.OIP_APP_MODE
      ? (process.env.OIP_APP_MODE === 'distributed' ? 'distributed' : 'standalone')
      : 'distributed';
    const fallbackConfig = createProxyConfig({}, fallbackMode);

    writeConfig(fallbackConfig);
    console.warn('Failed to fetch proxy config from API, fallback config was generated instead.');
    console.warn(error instanceof Error ? error.message : error);
    if (error && typeof error === 'object' && 'cause' in error) {
      console.warn(error.cause);
    }
  }
}

main().catch(error => {
  console.error(error);
  process.exit(1);
});
