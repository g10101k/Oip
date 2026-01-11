const {env} = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';

const PROXY_CONFIG = [
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
      "/api",
      "/swagger",
      "/health"
    ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    },
    ws: true
  },

]

module.exports = PROXY_CONFIG;
