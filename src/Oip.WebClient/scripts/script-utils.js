const { execFileSync } = require('child_process');

function getCommand(command) {
  return process.platform === 'win32' ? `${command}.cmd` : command;
}

function run(command, args = [], options = {}) {
  execFileSync(getCommand(command), args, {
    stdio: 'inherit',
    ...options
  });
}

function runNpm(args, options) {
  run('npm', args, options);
}

function runNpx(args, options) {
  run('npx', args, options);
}

module.exports = {
  runNpm,
  runNpx
};
