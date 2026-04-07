const {env} = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function getProxyConfig() {
  try {

    const r = await fetch( target + '/api/proxy-settings/get-spa-proxy-settings');
    const config = await r.json();


    const distributedProxy = [
      {
        context: [
          "/api/users",
          "/api/user-profile"
        ],
        target: config.targets.users,
        secure: false,
        headers: {
          Connection: 'Keep-Alive'
        },
        ws: true
      },
      {
        context: [
          "/hubs/notification"
        ],
        target: config.targets.notification,
        secure: false,
        changeOrigin: true,
        ws: true,
      },
      {
        context: [
          "/api/discussion"
        ],
        target: config.targets.discussion,
        secure: false,
        changeOrigin: true,
        ws: true,
      },
      {
        context: [
          "/api",
          "/swagger",
          "/health",
          "/metrics"
        ],
        target: target,
        secure: false,
        headers: {
          Connection: 'Keep-Alive'
        },
        ws: true
      }
    ];
    console.error(distributedProxy)
    return distributedProxy;
  } catch (error) {
    console.error(error);
    console.error('Failed to fetch proxy config from backend, using fallback');

    // Fallback на локальную конфигурацию
    const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
      env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';
    const appMode = env.OIP_APP_MODE === 'distributed' ? 'distributed' : 'standalone';

    const standaloneProxy = [
      {
        context: [
          "/api",
          "/swagger",
          "/health",
          "/metrics",
          "/hubs/notification"
        ],
        target: target,
        secure: false,
        headers: {
          Connection: 'Keep-Alive'
        },
        ws: true
      }
    ];

    const distributedProxy = [
      {
        context: [
          "/api/users",
          "/api/user-profile"
        ],
        target: "https://localhost:5005",
        secure: false,
        headers: {
          Connection: 'Keep-Alive'
        },
        ws: true
      },
      {
        context: [
          "/hubs/notification"
        ],
        target: "https://localhost:5007",
        secure: false,
        changeOrigin: true,
        ws: true,
      },
      {
        context: [
          "/api/discussion"
        ],
        target: "https://localhost:5006",
        secure: false,
        changeOrigin: true,
        ws: true,
      },
      {
        context: [
          "/api",
          "/swagger",
          "/health",
          "/metrics"
        ],
        target: target,
        secure: false,
        headers: {
          Connection: 'Keep-Alive'
        },
        ws: true
      }
    ];

    return appMode === 'distributed' ? distributedProxy : standaloneProxy;
  }
}

module.exports = getProxyConfig();
