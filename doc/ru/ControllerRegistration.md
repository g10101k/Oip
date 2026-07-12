# Регистрация контроллеров

## Описание

В OIP можно управлять тем, какие ASP.NET Core контроллеры будут опубликованы приложением.

По умолчанию управление регистрацией выключено. Если приложение вызывает только `AddControllersAndView()`, ASP.NET Core
использует стандартное обнаружение контроллеров и не фильтрует их.

Явная регистрация включается только когда используется `AddController<TController>()`. После первого такого вызова
приложение публикует только те контроллеры, которые были явно добавлены через `AddController<TController>()`.

## Когда использовать

Используйте `AddController<TController>()`, когда сервис должен публиковать ограниченный набор контроллеров из общих
сборок или модулей. В текущем репозитории это требуется когда Oip.Base используется в сервисах (Oip.Applications,
Oip.Users и тд.), в этих сервисах не должны быть доступны методы модулей (FolderModule, MenuController и др.).

Пример:

```csharp
builder.Services
    .AddControllersAndView()
    .AddController<ApplicationsController>()
    .AddController<SecurityController>();
```

В этом режиме контроллеры, которые доступны в подключенных сборках, но не добавлены через
`AddController<TController>()`,
не будут опубликованы.

Если не планируется запускать приложение в распределенном режиме IsStandalone = false или фильтровать регистрацию
контроллеров. Не используйте `AddController<TController>()`, `.AddApplicationControllers()` и
`.AddBaseServiceControllers()`

