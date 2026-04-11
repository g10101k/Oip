# Режимы приложения: standalone и distributed

Приложение разрабатывается с поддержкой двух архитектурных режимов:

- в `standalone` основные backend-модули работают внутри одного основного приложения.
- в `distributed` различные сервисы без фронтенда, работают как отдельные приложения.

> Важно: frontend не знает сколько сервисов запущено, он всегда работает с базовым URL. В `distributed` режиме
> настраивается прокси на разные URL, в `standalone` на один URL. Из-за этого реализовано динамическое формирование
> настроек proxy.

## Режим standalone

В режиме `standalone` основное приложение поднимает связанные модули локально внутри себя. В этом режиме:

- Основной backend остается главной точкой входа
- Вспомогательные сервисы `Oip.Users`, `Oip.Discussions` и `Oip.Notifications` запускаются внутри приложения.

## Режим distributed

В режиме `distributed` система рассматривается как набор отдельных сервисов. В этом режиме:

- основной backend остается центральной точкой для части API
- отдельные домены могут жить в собственных сервисах

## Как режим задается на backend

Ключевой флаг конфигурации `IsStandalone` - он используется как архитектурный переключатель.

Кроме `IsStandalone`, для генерации proxy.conf.js на backend используется список адресов сервисов:

- `Oip` -> `https://localhost:5002`
- `OipUsers` -> `https://localhost:5005`
- `OipDiscussions` -> `https://localhost:5006`
- `OipNotifications` -> `https://localhost:5007`

## Как режим влияет на backend-сборку приложения

В основном приложении режим влияет на то, как реально собирается runtime-конфигурация backend(DI). Это видно в
`Program.cs`:

- если `settings.IsStandalone == true`, основное приложение подключает `Oip.Users`, `Oip.Discussions` и
  `Oip.Notifications`  локально
- если `settings.IsStandalone == false`, приложение работает в более распределенной схеме, где часть функциональности
  ожидается во внешних сервисах.

## Как frontend узнает о текущем режиме

Frontend не читает `IsStandalone` напрямую. Вместо этого перед запуском dev-сервера выполняется скрипт:

- `./src/Oip.WebClient/scripts/generate-proxy-config.js`

Он получает настройки из backend `GET /api/proxy-settings/get-spa-proxy-settings` в виде target-адресов:

- `main`
- `users`
- `discussion`
- `notification`

Логика ответа такая:

- `main` всегда равен `appSettings.Services.Oip`
- в `standalone` адреса `users`, `discussion`, `notification` тоже сводятся к `appSettings.Services.Oip`
- в `distributed` эти адреса берутся из отдельных значений `Services`

После получения настроек frontend сравнивает target-адреса:

- если все адреса совпадают, он считает режим `standalone`
- если адреса различаются, он считает режим `distributed`

Дальше на основе этого автоматически генерируется файл `./src/Oip.WebClient/obj/proxy.generated.json` а
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

1. подготовка HTTPS-сертификата
2. генерация proxy-конфига - при этом синхронизируется с backend-средой, которая уже определяет
   архитектурный режим приложения. Если frontend не смог получить настройки у backend, создается fallback-конфиг в
   режиме `distributed` и используются сервера:
    - `main` -> `https://localhost:5002`
    - `users` -> `https://localhost:5005`
    - `discussion` -> `https://localhost:5006`
    - `notification` -> `https://localhost:5007`

