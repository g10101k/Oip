const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const projectRoot = path.resolve(__dirname, '..');
const outputDirectory = path.join(projectRoot, 'obj');
const outputFilePath = path.join(outputDirectory, 'proxy.generated.json');

const defaultTarget = process.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS ? process.env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5008';

async function loadProxyConfigFromApi() {
  let lastError;

  for (let attempt = 1; attempt <= 10; attempt += 1) {
    try {
      process.stderr.write(`Loading proxy settings from ${defaultTarget}, attempt ${attempt}/10\n`);

      const response = await fetch(`${defaultTarget}/api/proxy-settings/get-spa-proxy-settings`);

      if (!response.ok) {
        throw new Error(`Proxy settings request failed with status ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      lastError = error;
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
  }

  throw lastError ?? new Error('Failed to fetch proxy settings from API');
}

function writeConfig(config) {
  fs.mkdirSync(outputDirectory, { recursive: true });
  fs.writeFileSync(outputFilePath, JSON.stringify(config, null, 2));
}

async function main() {
  const proxyConfig = await loadProxyConfigFromApi();
  writeConfig(proxyConfig);
  console.log(`Proxy config generated from API: ${outputFilePath}`);
}

if (require.main === module) {
  main().catch(error => {
    console.error(error);
    process.exit(1);
  });
}

module.exports = main;
