#!/usr/bin/env node

const {execSync} = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🚀 Starting oip-common library publication...');

try {
  // 0. Increment version
  let commonPath = path.join(__dirname, '../projects/oip-common');
  let distPath = path.join(__dirname, '../dist/oip-common');
  execSync(`rm -rf ${distPath}`);

  // 1. Build the library
  console.log('📦 Building library...');
  execSync('ng build oip-common', {stdio: 'inherit'});

  // 2. Navigate to dist directory

  if (!fs.existsSync(distPath)) {
    throw new Error(`Directory ${distPath} not found!`);
  }

  console.log('🧹 Delete oip-common library...!');
  execSync('rm -rf ./node_modules/oip-common', {stdio: 'inherit'});

  console.log('🪃 Copy oip-common library...!');
  execSync(`cp -r ${distPath} ./node_modules/oip-common`);

  console.log('⛳ Publication completed successfully!');
} catch (error) {
  console.error('❌ Error during publication:', error.message);
  process.exit(1);
}
