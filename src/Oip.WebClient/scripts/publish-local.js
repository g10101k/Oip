#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const {runNpm, runNpx} = require('./script-utils');

console.log('🚀 Starting oip-common library publication...');

try {
  // 0. Increment version
  let distPath = path.join(__dirname, '../dist/oip-common');
  fs.rmSync(distPath, {recursive: true, force: true});

  // 1. Build the library
  console.log('📦 Building library...');
  runNpx(['ng', 'build', 'oip-common']);

  // 2. Navigate to dist directory
  if (!fs.existsSync(distPath)) {
    throw new Error(`Directory ${distPath} not found!`);
  }

  console.log('🧹 Delete oip-common library...!');
  fs.rmSync(path.join(__dirname, '../node_modules/oip-common'), {recursive: true, force: true});

  console.log('🪃 Install oip-common library...!');
  runNpm(['i', distPath, '--no-save']);

  console.log('⛳  Publication completed successfully!');
} catch (error) {
  console.error('❌ Error during publication:', error.message);
  process.exit(1);
}
