# OipCommon

Add assets in angular.json
```json
{
  "glob": "**/*",
  "input": "node_modules/oip-common/assets",
  "output": "/assets"
}
```

Add tailwind config

```js
const primeui = require('tailwindcss-primeui');
module.exports = {
  /* Your config */
  content: [/* Your config */, './node_modules/oip-common/**/*.{html,ts,scss,css,js,mjs}'],
  /* Your config */
};
```

Add scss
```sass
@use "../../../node_modules/oip-common/assets/oip-common";
```
