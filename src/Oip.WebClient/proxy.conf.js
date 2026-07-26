process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const fallbackTarget = process.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}` :
  process.env.ASPNETCORE_URLS ? process.env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5002';

function getUrl(name, defaultValue = '') {
  return process.env[`OIP_URLS:${name}`] ?? process.env[`OIP_URLS__${name}`] ?? defaultValue;
}

function createKeepAliveProxy(context, target) {
  return {
    context,
    target,
    secure: false,
    ws: true,
    changeOrigin: true,
  };
}

const shellTarget = getUrl('Shell', fallbackTarget);
const usersTarget = getUrl('UsersService');
const notificationsTarget = getUrl('NotificationsService');
const discussionsTarget = getUrl('DiscussionsService');
const applicationsTarget = getUrl('ApplicationsService');

const sharedKeepAliveContext = [
  '/manifest.json',
  '/api',
  '/signin-oidc',
  '/signout-callback-oidc',
  '/signout-oidc',
  '/swagger',
  '/health',
  '/metrics',
  '/hubs'
];

const hasDistributedTargets = Boolean(usersTarget)
  && Boolean(notificationsTarget)
  && Boolean(discussionsTarget)
  && Boolean(applicationsTarget);

module.exports = hasDistributedTargets ?
  [
    createKeepAliveProxy(['/api/users', '/api/user-profile'], usersTarget),
    createKeepAliveProxy(['/hubs/notification', '/api/notification'], notificationsTarget),
    createKeepAliveProxy(['/api/discussion'], discussionsTarget),
    createKeepAliveProxy(['/api/applications'], applicationsTarget),
    createKeepAliveProxy(sharedKeepAliveContext, shellTarget)] :
  [createKeepAliveProxy(sharedKeepAliveContext, shellTarget)];
