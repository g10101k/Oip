#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const webClientRoot = path.resolve(__dirname, '..');
const repoRoot = path.resolve(webClientRoot, '..');

const commonPropsPath = path.join(repoRoot, 'Common.props');
const webClientPackagePath = path.join(webClientRoot, 'package.json');
const commonPackagePath = path.join(webClientRoot, 'projects/oip-common/package.json');

const releaseTypes = ['major', 'minor', 'patch'];
const args = process.argv.slice(2);
const isDryRun = args.includes('--dry-run');
const isCheck = args.includes('--check');

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

function bump(version, releaseType) {
  switch (releaseType) {
    case 'major':
      return `${version.major + 1}.0.0`;
    case 'minor':
      return `${version.major}.${version.minor + 1}.0`;
    default:
      return `${version.major}.${version.minor}.${version.patch + 1}`;
  }
}

function relative(filePath) {
  return path.relative(repoRoot, filePath);
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

function readWebClientVersion() {
  return readJson(webClientPackagePath).version;
}

function writeWebClientVersion(version) {
  const packageJson = readJson(webClientPackagePath);
  packageJson.version = version;
  writeJson(webClientPackagePath, packageJson);
}

function readCommonPackageVersion() {
  return readJson(commonPackagePath).version;
}

function writeCommonPackageVersion(version) {
  const packageJson = readJson(commonPackagePath);
  packageJson.version = version;
  writeJson(commonPackagePath, packageJson);
}

// Common.props is the source of truth, every other target must repeat its version.
const targets = [
  { name: `${relative(commonPropsPath)} <Version>`, read: readNuGetVersion, write: writeNuGetVersion },
  { name: `${relative(webClientPackagePath)} version`, read: readWebClientVersion, write: writeWebClientVersion },
  { name: `${relative(commonPackagePath)} version`, read: readCommonPackageVersion, write: writeCommonPackageVersion },
];

function readTargets() {
  return targets.map((target) => {
    const version = target.read();

    if (!version) {
      throw new Error(`Version was not found in ${target.name}.`);
    }

    return { target, version };
  });
}

function findMismatched(current) {
  const [source] = current;

  return current.filter((item) => item.version !== source.version);
}

function check(current) {
  const mismatched = findMismatched(current);
  // Keep the report in the same stream as the outcome, otherwise CI logs interleave them.
  const report = current.map((item) => `${item.version} ${item.target.name}`).join('\n');

  if (mismatched.length > 0) {
    throw new Error(
      `${report}\nVersions are out of sync with ${current[0].target.name}. ` +
        `Run "npm run version:set -- <version>" to align them.`
    );
  }

  console.log(`${report}\nVersions are in sync.`);
}

function getTargetVersion(current) {
  const explicitVersion = args.find((arg) => !arg.startsWith('--') && !releaseTypes.includes(arg));

  if (explicitVersion) {
    return parseVersion(explicitVersion, 'command line argument').raw;
  }

  const mismatched = findMismatched(current);

  if (mismatched.length > 0) {
    throw new Error(
      `Versions are out of sync, cannot bump. Run "npm run version:check" and align them with an explicit version.`
    );
  }

  const releaseType = args.find((arg) => releaseTypes.includes(arg)) ?? 'patch';

  return bump(parseVersion(current[0].version, current[0].target.name), releaseType);
}

function main() {
  const current = readTargets();

  if (isCheck) {
    check(current);
    return;
  }

  const targetVersion = getTargetVersion(current);
  parseVersion(targetVersion, 'target version');

  if (!isDryRun) {
    for (const { target } of current) {
      target.write(targetVersion);
    }
  }

  const action = isDryRun ? 'Would bump' : 'Version bumped';
  const fileAction = isDryRun ? 'Would update' : 'Updated';

  console.log(`${action} ${current[0].version} -> ${targetVersion}`);

  for (const { target } of current) {
    console.log(`${fileAction} ${target.name}`);
  }
}

try {
  main();
} catch (error) {
  console.error(error.message);
  process.exit(1);
}
