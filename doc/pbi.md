**Цель**

Основной `oip` становится shell-приложением:

- отвечает за login/auth/session;
- держит общий layout из `oip-common`;
- строит меню;
- загружает список доступных модулей с backend;
- динамически подключает frontend-модули;
- передает им контекст: пользователь, tenant/application, module instance id, язык, тему, permissions.

Каждый инженерный модуль становится remote app:

- имеет собственный Angular app;
- может разрабатываться и деплоиться отдельно;
- экспортирует стандартный entrypoint;
- использует `oip-common` и SDK-контракт платформы.

---

**Ключевая идея**

Не надо делать так:

```ts
loadRemoteModule({
    remoteEntry: 'https://...',
    exposedModule: './Module'
})
```

напрямую из случайного места.

Лучше ввести понятие **Frontend Module Manifest**.

Например backend возвращает:

```json
{
  "code": "rtds-tags",
  "title": "RTDS Tags",
  "type": "FederatedRemote",
  "routePath": "rtds-tags/:id",
  "remoteEntryUrl": "https://localhost:50010/remoteEntry.js",
  "remoteName": "rtdsTags",
  "exposedModule": "./Routes",
  "entryKind": "routes",
  "requiredOipCommonVersion": "^0.4.0",
  "requiredShellVersion": ">=1.2.0",
  "permissions": [
    "Rtds.Tags.View"
  ],
  "enabled": true
}
```

Shell читает manifest и на его основе добавляет routes.

---

**Что экспортирует remote**

Я бы выбрал не компонент, а `Routes`.

То есть remote экспортирует:

```ts
export const remoteRoutes: Routes = [
    {
        path: '',
        component: TagsPageComponent
    },
    {
        path: 'details/:tagId',
        component: TagDetailsComponent
    }
];
```

Почему лучше `routes`, а не один `ModuleComponent`:

- remote может иметь вложенную навигацию;
- проще lazy-load;
- проще guards/resolvers;
- shell не знает внутреннюю структуру модуля;
- лучше масштабируется.

В shell это превращается примерно в:

```ts
{
    path: manifest.routePath, 
    loadChildren: () => loadFederatedRoutes(manifest)
}
```

---

**Контракт между Shell и Remote**

Самая важная часть. Remote не должен напрямую знать внутренности shell-приложения.

Внутри `oip-common` должен дать remote-модулю:

```ts
interface OipModuleContext {
    applicationCode: string;
    moduleCode: string;
    moduleInstanceId: number;
    userId: string;
    userName: string;
    roles: string[];
    permissions: string[];
    language: string;
    theme: string;
    apiBaseUrl: string;
}
```

И сервисы:

```ts
interface OipShellApi {
    getContext(): OipModuleContext;

    hasPermission(permission: string): boolean;

    showToast(message: OipToast): void;

    setPageTitle(title: string): void;

    navigate(url: string): void;

    reportError(error: unknown): void;
}
```

Remote использует только этот контракт, а не внутренние сервисы shell.

---

**Роль `oip-common`**

Сейчас `oip-common` у вас уже содержит layout, auth, API clients, компоненты, меню, страницы
config/applications/modules. 

---

**Версионирование**

Это критично.

У каждого remote должны быть:

```json
{
  "oipShellVersion": ">=1.4.0",
  "oipCommonVersion": "^0.4.0",
  "angularVersion": "^20.0.0"
}
```

Shell перед загрузкой проверяет совместимость.

Если несовместимо, вместо падения показывает нормальную ошибку:

> Модуль RTDS Tags требует OIP Shell >= 1.4.0, текущая версия 1.3.2.

И логирует это в backend/observability.

---

**Shared dependencies**

Для Angular remotes надо шарить:

- `@angular/core`
- `@angular/common`
- `@angular/router`
- `@angular/forms`
- `rxjs`
- `primeng`
- `@primeng/themes`
- `oip-common`
- `@ngx-translate/core`

Важно: Angular лучше держать singleton. Иначе будут странные DI/runtime ошибки.

Концептуально:

``` js
shared: {
    '@angular/core'
:
    {
        singleton: true, strictVersion
    :
        true
    }
,
    '@angular/common'
:
    {
        singleton: true, strictVersion
    :
        true
    }
,
    '@angular/router'
:
    {
        singleton: true, strictVersion
    :
        true
    }
,
    'rxjs'
:
    {
        singleton: true
    }
,
    'oip-common'
:
    {
        singleton: true, strictVersion
    : 
        true
    }
}
```

---

**Backend-модель**

У вас уже есть `Applications` и `Modules`. Я бы добавил frontend integration metadata не в хаотичные поля, а явно.

Например:

```csharp
public enum FrontendIntegrationType
{
    InternalRoute = 0,
    Iframe = 1,
    FederatedRemote = 2,
    WebComponent = 3
}
```

Для federated remote:

```csharp
public class FrontendRemoteDefinition
{
    public string Code { get; set; }
    public string RoutePath { get; set; }
    public string RemoteEntryUrl { get; set; }
    public string RemoteName { get; set; }
    public string ExposedModule { get; set; }
    public string EntryKind { get; set; } // routes/component
    public string RequiredShellVersion { get; set; }
    public string RequiredOipCommonVersion { get; set; }
    public bool Enabled { get; set; }
}
```

И API:

```text
get-frontend-module-manifests
get-frontend-module-manifest-by-code/{code}
```

По вашим правилам роутинга это должно быть action-style route.

---

**Routing flow**

Поток такой:

1. Пользователь открывает OIP.
2. Shell логинится и получает пользователя.
3. Shell вызывает backend: `get-frontend-module-manifests`.
4. Backend возвращает только доступные пользователю модули.
5. Shell валидирует manifest.
6. Shell добавляет dynamic routes.
7. Меню строится на основе module instances.
8. При переходе в пункт меню shell lazy-load remote routes.
9. Remote получает `OipModuleContext`.
10. Remote работает как обычный Angular feature area.

---

**Обработка ошибок**

Обязательно нужны fallback states:

- remote недоступен;
- manifest битый;
- версия несовместима;
- remote загрузился, но экспорт неправильный;
- пользователь потерял permission;
- remote runtime error.

Для этого shell должен оборачивать загрузку:

```ts
loadChildren: async () => {
    try {
        return await remoteLoader.loadRoutes(manifest);
    } catch (error) {
        shellErrorReporter.reportRemoteLoadError(manifest, error);
        return [
            {
                path: '',
                component: RemoteLoadErrorComponent
            }
        ];
    }
}
```

Remote error не должен валить весь OIP.

---

**Security**

Module Federation не является security boundary. Это важно.

Remote JS выполняется в том же browser context, что и shell. Значит:

- remote может получить доступ к JS runtime;
- remote потенциально может читать shared services;
- нельзя подключать недоверенный remote;
- remoteEntry URLs должны быть allowlisted;
- желательно иметь подпись/хеш manifest;
- CSP надо адаптировать под remote origins;
- permissions проверять на backend, не только на frontend.

---

**Локальная разработка**

Удобный DX можно сделать так:

```text
OIP shell:       https://localhost:50002
RTDS remote:    https://localhost:50010
Other remote:   https://localhost:50011
Backend:        https://localhost:50001
```

В dev registry можно хранить remote URLs в config/db/env.

Инженер запускает:

```bash
npm run start:rtds-tags
```

Shell подхватывает:

```json
"remoteEntryUrl": "https://localhost:50010/remoteEntry.js"
```

И модуль появляется в меню без пересборки shell.

---

**CI/CD**

Для каждого remote:

1. Build remote app.
2. Publish static assets.
3. Publish `remoteEntry.js`.
4. Publish `frontend-module.manifest.json`.
5. Register/update manifest in OIP backend.
6. Smoke test загрузки remote.
7. Optional: compatibility check с текущим shell.

Желательно, чтобы remote сам отдавал manifest:

```text
https://module-host/manifest.json
https://module-host/remoteEntry.js
```

А backend OIP хранил ссылку и кешировал проверенный manifest.

---

**Как бы я внедрял у вас**

Я бы делал в 4 этапа.

**Этап 1: Спецификация manifest и SDK**

Без Module Federation кода сначала зафиксировать контракт:

- `FrontendIntegrationType`;
- `FrontendRemoteManifestDto`;
- `OipModuleContext`;
- `OipShellApi`;
- version fields;
- error states.

**Этап 2: Shell loader**

В `projects/oip` добавить сервис:

```text
FederatedRemoteRegistryService
FederatedRouteLoaderService
RemoteCompatibilityService
```

И научить app routes добавлять runtime routes из backend.

**Этап 3: Первый remote на базе `oip`**

`oip` хороший кандидат для пилота. Сейчас он отдельное Angular app. Его можно превратить или дополнить как remote 
app:

- оставить standalone запуск;
- добавить federation entrypoint;
- экспортировать `remoteRoutes`;
- проверить общий layout/auth/menu/context.

**Этап 4: Developer template**

Сделать шаблон:

```text
projects/oip-module-template
```

Команда:

```bash
npm run generate:module my-module
```

Создает:

- Angular remote app;
- manifest;
- routes entrypoint;
- example page;
- подключение `oip-common`;
- dev port;
- README для инженера.

---

**Мой вывод**

Для OIP я бы выбрал такую целевую модель:

- `InternalRoute` — для core-страниц внутри shell.
- `FederatedRemote` — для доверенных инженерных Angular-модулей.
- `Iframe` — для внешних/экспериментальных/не-Angular приложений.
- `Schema UI` — позже для простых CRUD-модулей.

А первым практическим шагом я бы сделал **не саму federation-сборку**, а manifest + SDK contract. Это фундамент. После
него и iframe, и Module Federation, и Web Components начинают подключаться одинаково через один реестр модулей.