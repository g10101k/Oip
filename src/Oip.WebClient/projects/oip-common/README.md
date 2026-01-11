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
const primeui = require("tailwindcss-primeui");
module.exports = {
  /* Your config */
  content: [, /* Your config */ "./node_modules/oip-common/**/*.{html,ts,scss,css,js,mjs}"]
  /* Your config */
};
```

Add scss

```sass
@use "../../../node_modules/oip-common/assets/oip-common";
```

Init L10nService to AppComponent

```ts
import { Component, inject, OnInit } from "@angular/core";
import { SecurityService } from "oip-common";
import { RouterOutlet } from "@angular/router";
import { ToastModule } from "primeng/toast";
import { L10nService } from "../../../oip-common/src/services/l10n.service";

@Component({
  selector: "app-root",
  template: `
    <p-toast />
    <router-outlet></router-outlet>
  `,
  standalone: true,
  imports: [ToastModule, RouterOutlet]
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);
  private readonly translateService = inject(L10nService);

  ngOnInit() {
    this.securityService.auth();
    this.translateService.init([
      {
        code: "en",
        name: "English",
        icon: "flag flag-gb"
      },
      {
        code: "ru",
        name: "Русский",
        icon: "flag flag-ru"
      }
    ]);
  }
}
```
