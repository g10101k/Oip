#!/usr/bin/env node

const fs = require('fs');
const os = require('os');
const path = require('path');
const {execFileSync} = require('child_process');
const {runNpm, runNpx} = require('./script-utils');

console.log('🚀 Starting oip-common library publication...');

function getCommand(command) {
  return process.platform === 'win32' ? `${command}.cmd` : command;
}

try {
  // 0. Increment version
  let distPath = path.join(__dirname, '../dist/oip-common');
  let nodeModulesPath = path.join(__dirname, '../node_modules/oip-common');
  let angularCachePath = path.join(__dirname, '../.angular/cache');

  console.log('🧹 Delete previous dist...');
  fs.rmSync(distPath, {recursive: true, force: true});

  // 1. Build the library
  console.log('📦 Building library...');
  runNpx(['ng', 'build', 'oip-common']);

  // 2. Navigate to dist directory
  if (!fs.existsSync(distPath)) {
    throw new Error(`Directory ${distPath} not found!`);
  }

  console.log('🧹 Delete oip-common library...!');
  fs.rmSync(nodeModulesPath, {recursive: true, force: true});

  console.log('🧹 Delete Angular cache...!');
  fs.rmSync(angularCachePath, {recursive: true, force: true});

  console.log('📦 Pack oip-common library...!');
  const packDirectory = fs.mkdtempSync(path.join(os.tmpdir(), 'oip-common-local-'));
  const npmEnv = {
    ...process.env,
    npm_config_cache: path.join(packDirectory, '.npm-cache')
  };
  const packageFileName = execFileSync(getCommand('npm'), ['pack', '--pack-destination', packDirectory], {
    cwd: distPath,
    env: npmEnv,
    encoding: 'utf8'
  }).trim();
  const packagePath = path.join(packDirectory, packageFileName);

  console.log('🪃 Install oip-common library...!');
  runNpm(['i', packagePath, '--no-save', '--force'], {env: npmEnv});

  fs.rmSync(packDirectory, {recursive: true, force: true});

  console.log('⛳  Publication completed successfully!');
} catch (error) {
  console.error('❌ Error during publication:', error.message);
  process.exit(1);
}
