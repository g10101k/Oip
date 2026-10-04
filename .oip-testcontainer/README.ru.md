# Контейнер для UI-тестов

[English](README.md) | **Русский**

Поднимает приложение Oip (сервисы перенесены из `../.oip-devcontainer/test.yml`) и Selenium Grid (hub + Chrome-нода)
для запуска `Oip.UiTest`. Сертификаты и креды — уже существующие dev-заглушки Oip
(`../.oip-devcontainer/https`), никаких секретов от других проектов сюда не переносилось.

## Запуск контейнеров

```shell
docker compose up -d --build --force-recreate
```

Без пересборки:

```shell
docker compose up -d
```

## Запустить UI-тесты в консоли

```shell
dotnet test ./../src/Oip.UiTest/Oip.UiTest.csproj --settings ./settings/default.runsettings
```

По умолчанию (без `--settings`) `TestSetup` поднимает локальный `ChromeDriver` и ходит на
`https://localhost:50000` — то есть тесты можно гонять и без Docker вовсе, если приложение уже
запущено локально. `--settings ./settings/default.runsettings` переключает тесты на Selenium Grid
из этого docker-compose (`RemoteDriverUrl`) и на адрес сервиса `oip` внутри docker-сети (`BaseUrl`).

## Тестовые пользователи

В realm `oip` (`../.oip-devcontainer/keycloak/realm-export.json`) два пользователя, у обоих пароль `P@ssw0rd`:

| Пользователь | Роль realm | Параметры запуска              |
|--------------|------------|--------------------------------|
| `admin`      | `admin`    | `Username`, `Password`         |
| `user`       | `user`     | `UserUsername`, `UserPassword` |

Тесты входят под `admin`; `user` нужен для проверки того, что доступно пользователю без прав администратора. Keycloak
импортирует realm только в пустую базу, поэтому на уже существующем volume dev-контейнера пользователя `user` нужно
создать вручную или пересоздать volume базы Keycloak.

## Объектное хранилище и загружаемые файлы

Фото пользователей хранятся в MinIO (сервис `minio`), bucket приложение создаёт при первой загрузке. `UserProfileTests`
загружают `src/Oip.UiTest/TestData/avatar.png`: браузер берёт файл из `BaseDirectory` — при локальном запуске это
папка сборки тестов, а для Selenium Grid — `/home/seluser/uploads`, куда в Chrome-ноду смонтирована папка
`src/Oip.UiTest`.

## Серверы, которые поднимают тесты

`ModuleRegistryTests` регистрируют модуль-расширение по манифесту, который отдаёт сам тестовый процесс: бэкенд скачивает
его с `http://<TestHost>:<случайный порт>/manifest.json`. По умолчанию `TestHost` — `localhost`, в `default.runsettings` —
`host.docker.internal`, который контейнер `oip` разрешает в адрес хоста. `SessionsTests` открывают второй браузер,
поэтому Chrome-нода допускает две сессии (`SE_NODE_MAX_SESSIONS`).

## Если кончилось место

```shell
docker builder prune -af
```

## Просмотр записей тестов

Видео прогонов складывается в анонимный volume `/videos` контейнера `chrome-video` и доступно через `file-browser` на
`http://localhost:8081`. `docker compose down -v` удаляет их вместе с контейнерами.
