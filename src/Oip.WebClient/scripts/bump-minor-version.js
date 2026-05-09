#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const webClientRoot = path.resolve(__dirname, '..');
const repoRoot = path.resolve(webClientRoot, '..');

const commonPropsPath = path.join(repoRoot, 'Common.props');
const webClientPackagePath = path.join(webClientRoot, 'package.json');
const commonPackagePath = path.join(webClientRoot, 'projects/oip-common/package.json');
const args = process.argv.slice(2);
const isDryRun = args.includes('--dry-run');

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, 'utf8'));
}

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function parseVersion(version, source) {
  const match = /^(\d+)\.(\d+)\.(\d+)(?:[-+].*)?$/.exec(version);

  if (!match) {
    throw new Error(`Unsupported version "${version}" in ${source}. Expected SemVer format like 0.2.0.`);
  }

  return {
    major: Number(match[1]),
    minor: Number(match[2]),
    patch: Number(match[3]),
    raw: version,
  };
}

function compareVersions(left, right) {
  return left.major - right.major || left.minor - right.minor || left.patch - right.patch;
}

function nextMinor(version) {
  return `${version.major}.${version.minor + 1}.0`;
}

function readNuGetVersion() {
  const content = fs.readFileSync(commonPropsPath, 'utf8');
  const match = /<Version>([^<]+)<\/Version>/.exec(content);

  if (!match) {
    throw new Error(`Version element was not found in ${commonPropsPath}.`);
  }

  return match[1];
}

function writeNuGetVersion(version) {
  const content = fs.readFileSync(commonPropsPath, 'utf8');
  const updated = content.replace(/<Version>[^<]+<\/Version>/, `<Version>${version}</Version>`);

  fs.writeFileSync(commonPropsPath, updated);
}

function getTargetVersion() {
  const explicitVersion = args.find((arg) => !arg.startsWith('--'));

  if (explicitVersion) {
    return parseVersion(explicitVersion, 'command line argument').raw;
  }

  const versions = [
    parseVersion(readNuGetVersion(), commonPropsPath),
    parseVersion(readJson(commonPackagePath).version, commonPackagePath),
  ];

  versions.sort(compareVersions);
  return nextMinor(versions[versions.length - 1]);
}

function updateWebClientDependency(version) {
  const packageJson = readJson(webClientPackagePath);

  if (packageJson.dependencies && packageJson.dependencies['oip-common']) {
    packageJson.dependencies['oip-common'] = version;
  }

  if (packageJson.devDependencies && packageJson.devDependencies['oip-common']) {
    packageJson.devDependencies['oip-common'] = version;
  }

  writeJson(webClientPackagePath, packageJson);
}

function updateCommonPackage(version) {
  const packageJson = readJson(commonPackagePath);
  packageJson.version = version;
  writeJson(commonPackagePath, packageJson);
}

function main() {
  const targetVersion = getTargetVersion();
  parseVersion(targetVersion, 'target version');

  if (!isDryRun) {
    writeNuGetVersion(targetVersion);
    updateCommonPackage(targetVersion);
    updateWebClientDependency(targetVersion);
  }

  const action = isDryRun ? 'Would bump' : 'Version bumped';
  const fileAction = isDryRun ? 'Would update' : 'Updated';

  console.log(`${action} to ${targetVersion}`);
  console.log(`${fileAction} ${path.relative(repoRoot, commonPropsPath)}`);
  console.log(`${fileAction} ${path.relative(repoRoot, commonPackagePath)}`);
  console.log(`${fileAction} ${path.relative(repoRoot, webClientPackagePath)}`);
}

try {
  main();
} catch (error) {
  console.error(error.message);
  process.exit(1);
}
