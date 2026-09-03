#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const {execFileSync} = require('child_process');
const {runNpm, runNpx} = require('./script-utils');

console.log('🚀 Starting oip-common library publication...');

try {
  // 0. The version is owned by Common.props, see scripts/bump-version.js
  console.log('🔍 Checking that npm and NuGet versions are in sync...');
  execFileSync(process.execPath, [path.join(__dirname, 'bump-version.js'), '--check'], {stdio: 'inherit'});

  // 1. Build the library
  console.log('📦 Building library...');
  runNpx(['ng', 'build', 'oip-common']);

  // 2. Navigate to dist directory
  let distPath = path.join(__dirname, '../dist/oip-common');

  if (!fs.existsSync(distPath)) {
    throw new Error(`Directory ${distPath} not found!`);
  }

  // 3. Read package.json to display version
  const packageJson = JSON.parse(fs.readFileSync(path.join(distPath, 'package.json'), 'utf8'));
  console.log(`📋 Version for publication: ${packageJson.version}`);

  console.log('🗝️ Login...');
  runNpm(['login'], {cwd: distPath});

  // 4. Publish
  console.log('📤 Publishing to npm...');
  runNpm(['publish'], {cwd: distPath});

  console.log('✅ Publication completed successfully!');
} catch (error) {
  console.error('❌ Error during publication:', error.message);
  process.exit(1);
}
