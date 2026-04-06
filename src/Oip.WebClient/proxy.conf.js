const {env} = require('process');

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

const PROXY_CONFIG = appMode === 'distributed' ? distributedProxy : standaloneProxy;

module.exports = distributedProxy;
