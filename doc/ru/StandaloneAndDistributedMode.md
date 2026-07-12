# Режимы приложения: standalone и distributed

Приложение разрабатывается с поддержкой двух архитектурных режимов:

- `standalone` - основные backend-модули работают внутри одного основного приложения.
- `distributed` - часть функциональности вынесена в отдельные сервисы.

> Важно: frontend не знает сколько сервисов запущено, он всегда работает с базовым URL. В `distributed` режиме
> настраивается прокси на разные URL, в `standalone` на один URL. Из-за этого реализовано динамическое формирование
> настроек proxy.

## Режим standalone

В режиме `standalone` основное приложение поднимает связанные модули локально внутри себя. В этом режиме:

- основной backend остается главной точкой входа;
- модули `Oip.Users`, `Oip.Discussions`, `Oip.Notifications` и `Oip.Applications` подключаются локально;
- для основного приложения `ServiceAddingMode` имеет значение `Local`.

## Режим distributed

В режиме `distributed` система работает как набор отдельных приложений и сервисов. В этом режиме:

- основной backend остается центральной точкой для части API;
- отдельные домены могут жить в собственных сервисах;
- основное приложение использует `ServiceAddingMode = Remote` для удаленных модулей;
- сами выделенные сервисы обычно запускаются с `ServiceAddingMode = Service`.

## Как режим задается на backend

Ключевой флаг конфигурации - `ServiceAddingMode`, он указывает как именно будут подключаться сервисы в приложении.

Для понимания режимов на уровне системы значения интерпретируются так:

- `Local` - модуль подключается локально в основное приложение; это соответствует режиму `standalone`;
- `Remote` - основное приложение ожидает модуль как внешний сервис; это соответствует режиму `distributed` для
  потребителя;
- `Service` - модуль запускается как отдельный backend-сервис и предоставляет API/gRPC другим частям системы.

Кроме `ServiceAddingMode`, для генерации proxy на backend используется список адресов сервисов:

- `Oip` -> `https://localhost:5002`
- `OipApplications` -> `https://localhost:5008`
- `OipUsers` -> `https://localhost:5005`
- `OipDiscussions` -> `https://localhost:5006`
- `OipNotifications` -> `https://localhost:5007`

## Как режим влияет на backend-сборку приложения

В основном приложении режим влияет на runtime-конфигурацию DI и подключение модулей:

- если `settings.ServiceAddingMode == AddingMode.Local`, основное приложение подключает `Oip.Users`, `Oip.Discussions`,
  `Oip.Notifications` и `Oip.Applications` локально;
- если `settings.ServiceAddingMode == AddingMode.Remote`, приложение работает в распределенной схеме и ожидает часть
  функциональности во внешних сервисах.

Для отдельных сервисов значение `AddingMode.Service` означает, что сервис поднимается как самостоятельное приложение и
экспортирует свою функциональность наружу.

## Как frontend узнает о текущем режиме

Frontend не читает `ServiceAddingMode` напрямую. Вместо этого перед запуском dev-сервера выполняется скрипт:

- `./src/Oip.WebClient/scripts/generate-proxy-config.js`

Он получает настройки из backend `GET /api/proxy-settings/get-spa-proxy-settings` в виде target-адресов:

- `main`
- `applications`
- `users`
- `discussion`
- `notification`

Логика ответа такая:

- `main` всегда равен `appSettings.Services.Oip`;
- в `standalone` адреса `applications`, `users`, `discussion`, `notification` тоже сводятся к
  `appSettings.Services.Oip`;
- в `distributed` эти адреса берутся из отдельных значений `Services`.

Backend при этом использует правило:

- если `ServiceAddingMode == Local`, proxy-цели сервисов указывают на основной backend;
- иначе используются отдельные адреса соответствующих сервисов.

После получения настроек frontend сравнивает target-адреса:

- если все адреса совпадают, он считает режим `standalone`;
- если адреса различаются, он считает режим `distributed`.

Дальше на основе этого автоматически генерируется файл `./src/Oip.WebClient/obj/proxy.generated.json`, а
`./src/Oip.WebClient/proxy.conf.js` читает готовый результат.

## Какие proxy-маршруты получаются как следствие

В `standalone` все основные маршруты идут в основной backend.

В `distributed` маршруты разделяются по сервисам:

- `/api/users`, `/api/user-profile` -> users service
- `/hubs/notification` -> notifications service
- `/api/discussion` -> discussions service
- `/api`, `/swagger`, `/health`, `/metrics` -> main service

## Что происходит при обычном запуске frontend

В `package.json` перед `start` автоматически выполняется:

1. подготовка HTTPS-сертификата;
2. генерация proxy-конфига, при которой frontend синхронизируется с backend-средой. Если frontend не смог получить
   настройки у backend, создается fallback-конфиг в режиме `distributed` и используются
   сервера:

- `main` -> `https://localhost:5002`
- `applications` -> `https://localhost:5008`
- `users` -> `https://localhost:5005`
- `discussion` -> `https://localhost:5006`
- `notification` -> `https://localhost:5007`
