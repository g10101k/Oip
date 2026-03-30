#!/usr/bin/env node

const {execSync} = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🚀 Starting oip-common library publication...');

try {
  // 0. Increment version
  let commonPath = path.join(__dirname, '../projects/oip-common');

  console.log('⬆️ Incrementing patch version...');
  execSync('npm version patch', {cwd: commonPath, stdio: 'inherit'});

  // 1. Build the library
  console.log('📦 Building library...');
  execSync('ng build oip-common', {stdio: 'inherit'});

  // 2. Navigate to dist directory
  let distPath = path.join(__dirname, '../dist/oip-common');

  if (!fs.existsSync(distPath)) {
    throw new Error(`Directory ${distPath} not found!`);
  }

  // 3. Read package.json to display version
  const packageJson = JSON.parse(fs.readFileSync(path.join(distPath, 'package.json'), 'utf8'));
  console.log(`📋 Version for publication: ${packageJson.version}`);

  console.log('🗝️ Login...');
  execSync('npm login', {cwd: distPath, stdio: 'inherit'});

  // 4. Publish
  console.log('📤 Publishing to npm...');
  execSync('npm publish', {cwd: distPath, stdio: 'inherit'});

  console.log('✅ Publication completed successfully!');
} catch (error) {
  console.error('❌ Error during publication:', error.message);
  process.exit(1);
}
