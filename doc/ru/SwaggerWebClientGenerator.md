# Swagger WebClient Generator

## Описание

`SwaggerGenerateWebClientStartupTask` генерирует TypeScript web api клиент из Swagger-документов при старте ASP.NET
Core приложения.

Задача запускается только когда она зарегистрирована в DI:

- в сервисах, где используется `GenerateWebClientStartupTask(settings)`, регистрация включается ключом
  `GenerateWebClient`
- обрабатываются только элементы `OpenApi`, у которых заполнен `GenerateCommand`

После выполнения генерации задача вызывает `StopApplication()`, поэтому приложение не продолжает обычный запуск.

Важно: сгенерированные frontend API файлы считаются read-only. Их нужно обновлять только через регенерацию, а не
ручными правками.

Пример запуска генерации:
```shell
dotnet run ./Oip.csproj --no-restore -- --GenerateWebClient=true
```

## Как это работает

При запуске приложения с аргументом `GenerateWebClient`, `GenerateWebClientStartupTask` выполняет такой сценарий:

1. Берет все записи из `settings.OpenApi`, где указан `GenerateCommand`.
2. Для каждой записи получает Swagger JSON.
3. Сохраняет Swagger JSON в файл.
4. Запускает внешнюю команду генерации, передавая путь к файлу через `{SwaggerJsonPath}`.
5. Удаляет файл Swagger JSON.

Если по конкретной конфигурации произошла ошибка, она логируется, после чего ошибка пробрасывается дальше.
После завершения всегда пишется лог `Swagger generation complete`, затем приложение останавливается через
`StopApplication()`.

## Где хранится Swagger

Swagger JSON сохраняется только как временный входной файл для команды генерации.

Файл создается по пути:

```text
obj/SwaggerFiles/swagger-{name}.json
```

Пример:

```text
obj/SwaggerFiles/swagger-base.json
obj/SwaggerFiles/swagger-v1.json
```

После выполнения команды файл удаляется.

## Конфигурация `OpenApi`

Ключевые поля `OpenApiItem`:

- `Name` - имя swagger-группы, которое используется в `GetSwagger(config.Name)`
- `Publish` - публикация swagger-документа
- `Url` - URL swagger json
- `Version` - версия API
- `Title` - заголовок API
- `Description` - описание API
- `GenerateCommand` - команда генерации web api клиента
- `ForceGeneration` - устаревшее поле; 

Для того чтобы контроллер попал в нужную swagger-группу, нужно использовать атрибут:

```csharp
[ApiExplorerSettings(GroupName = "base")]
```

Значение `GroupName` должно совпадать с `OpenApi[].Name`.

## Пример `appsettings.Development.json`

Актуальный формат настройки генерации выглядит так:

```json
{
  "OpenApi": [
    {
      "Publish": true,
      "Name": "v1",
      "Url": "/swagger/v1/swagger.json",
      "Version": "v1.0.0",
      "Title": "Oip service web-api",
      "Description": "Oip service web-api",
      "GenerateCommand": "node ./node_modules/oip-common/scripts/generate-api.mjs -c -i {SwaggerJsonPath} -t ./node_modules/oip-common/templates -o ./projects/oip/src/api"
    },
    {
      "Publish": true,
      "Name": "base",
      "Url": "/swagger/base/swagger.json",
      "Version": "v1.0.0",
      "Title": "Base service web-api",
      "Description": "Base service web-api",
      "GenerateCommand": "node ./node_modules/oip-common/scripts/generate-api.mjs -i {SwaggerJsonPath} -t ./node_modules/oip-common/templates -o ./projects/oip-common/src/api"
    }
  ]
}
```

## Как выполняется `GenerateCommand`

`GenerateCommand` запускается как внешний процесс.

Особенности выполнения:

- перед запуском placeholder `{SwaggerJsonPath}` заменяется на путь к временному Swagger JSON
- если путь содержит пробелы, он автоматически оборачивается в кавычки
- рабочая директория процесса берется из `settings.SpaProxyServer.WorkingDirectory`
- в окружение процесса добавляется `NODE_TLS_REJECT_UNAUTHORIZED=0`
- `stdout` пишется в `ILogger` как `Information`
- `stderr` пишется в `ILogger` как `Error`

На Windows команда дополнительно запускается через:

```text
cmd.exe /c
```

Поэтому команда должна быть задана так, чтобы ее можно было выполнить из `SpaProxyServer.WorkingDirectory`.

## Логика `generate-api.mjs`

В текущем проекте `GenerateCommand` обычно вызывает скрипт:

```text
./projects/oip-common/scripts/generate-api.mjs
```

Этот скрипт является оберткой над `swagger-typescript-api` и задает общие правила генерации для frontend.

### Поддерживаемые аргументы

Скрипт принимает основные параметры:

- `-i`, `--input` - путь к локальному swagger json
- `-o`, `--output` - папка для генерации файлов
- `-t`, `--templates` - путь к кастомным шаблонам
- `-d`, `--data-contract-prefix` - префикс для файла `data-contracts`, позволяет сгенерировать в один каталог несколько
  различных api.
- `-c`, `--use-common-client` - использовать общий http client вместо генерации собственного

### Базовые настройки генерации

Скрипт вызывает `generateApi()` со следующими важными параметрами:

- `httpClientType: "fetch"`
- `generateResponses: true`
- `extractRequestParams: true`
- `extractRequestBody: true`
- `extractEnums: true`
- `unwrapResponseData: true`
- `modular: true`
- `moduleNameFirstTag: true`
- `cleanOutput: false`
- `defaultResponseType: "void"`

Это означает, что генерация:

- строится на `fetch`
- выносит request params, request body, enums и response types в отдельные структуры
- генерирует модульный клиент
- не очищает output-директорию перед запуском
- использует первый swagger tag для имени модуля

### Кастомизация типов

В скрипте заданы дополнительные преобразования:

- OpenAPI `string` с форматом `date-time` преобразуется в TypeScript тип `Date`
- для исправления невалидных имен используются префиксы `Type` и `Value`

Также определен кастомный `RecordType`, который возвращает:

```ts
MyRecord<key, value>
```

Если шаблоны и типы проекта завязаны на `MyRecord`, это поведение нужно учитывать при изменении генератора.

### Правила именования файлов

После генерации скрипт дополнительно переименовывает выходные файлы:

- `data-contracts.ts` сохраняется как `{dataContractPrefix}data-contracts.ts`
- `http-client.ts` сохраняется как есть
- остальные файлы получают имя в `kebab-case` и суффикс `.api`

Примеры:

- `UserProfile` -> `user-profile.api.ts`
- `Security` -> `security.api.ts`

dataContractPrefix

### Режим `use common client`

Если передан флаг `-c` / `--use-common-client`, то:

- в конфигурацию генератора передается `useCommonClient: true`
- файл `http-client.ts` не записывается на диск

В этом режиме frontend использует общий HTTP client из инфраструктуры проекта.

### Запись файлов

Скрипт:

- резолвит `input`, `templates` и `output` относительно `process.cwd()`
- при необходимости создает output-директорию
- записывает каждый сгенерированный файл отдельно
- пишет в консоль служебные сообщения вида `File create: ...`

Поскольку `cleanOutput: false`, старые файлы, которые больше не генерируются, автоматически не удаляются. Это важно
учитывать при изменениях структуры API или шаблонов.

## Что устарело

Документация и настройки не должны опираться на `WebClientOutputPath`, `ForceGeneration` или постоянный snapshot
Swagger.

В текущей реализации генерация управляется только через `GenerateCommand`. Именно эта команда определяет:

- какой генератор используется
- какие шаблоны применяются
- в какую папку пишется результат
- какие дополнительные флаги включены

Иными словами, логика генерации сейчас задается конфигурацией команды, а не отдельным полем output path.
