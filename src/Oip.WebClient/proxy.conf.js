const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const generatedProxyConfigPath = path.join(__dirname, 'obj', 'proxy.generated.json');

try {
  if (!fs.existsSync(generatedProxyConfigPath)) {
    throw new Error(`Generated proxy config not found: ${generatedProxyConfigPath}`);
  }

  module.exports = JSON.parse(fs.readFileSync(generatedProxyConfigPath, 'utf8'));
} catch (error) {
  console.error('Failed to load generated proxy config.', error);
  throw error;
}
