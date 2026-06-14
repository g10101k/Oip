const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const generatedProxyConfigPath = path.join(__dirname, 'obj', 'proxy.generated.json');

module.exports = JSON.parse(fs.readFileSync(generatedProxyConfigPath, 'utf8'));

