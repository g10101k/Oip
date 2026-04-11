const fs = require('fs');
const path = require('path');
const {env} = require('process');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const generatedProxyConfigPath = path.join(__dirname, 'obj', 'proxy.generated.json');
const defaultTarget = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';

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

function createFallbackConfig() {
  const appMode = env.OIP_APP_MODE === 'standalone' ? 'standalone' : 'distributed';

  const standaloneProxy = [
    createKeepAliveProxy(
      [
        '/api',
        '/swagger',
        '/health',
        '/metrics',
        '/hubs/notification'
      ],
      defaultTarget
    )
  ];

  const distributedProxy = [
    createKeepAliveProxy(
      [
        '/api/users',
        '/api/user-profile'
      ],
      env.OIP_USERS_TARGET || 'https://localhost:5005'
    ),
    createWsProxy(
      [
        '/hubs/notification'
      ],
      env.OIP_NOTIFICATION_TARGET || 'https://localhost:5007'
    ),
    createWsProxy(
      [
        '/api/discussion'
      ],
      env.OIP_DISCUSSION_TARGET || 'https://localhost:5006'
    ),
    createKeepAliveProxy(
      [
        '/api',
        '/swagger',
        '/health',
        '/metrics'
      ],
      defaultTarget
    )
  ];

  return appMode === 'distributed' ? distributedProxy : standaloneProxy;
}

try {
  if (fs.existsSync(generatedProxyConfigPath)) {
    module.exports = JSON.parse(fs.readFileSync(generatedProxyConfigPath, 'utf8'));
  } else {
    module.exports = createFallbackConfig();
  }
} catch (error) {
  console.error('Failed to load generated proxy config, using fallback.', error);
  module.exports = createFallbackConfig();
}
