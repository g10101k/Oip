# Контейнер для UI-тестов

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

## Если кончилось место

```shell
docker builder prune -af
```

## Просмотр записей тестов

Видео прогонов складывается в volume `tests_video` и доступно через `file-browser` на
`http://localhost:8081`.
