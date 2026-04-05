# 🧩 Swagger WebClient Generator

## 📖 Описание

`SwaggerGenerateWebClientStartupTask` — это **стартап-таск** для ASP.NET Core, который автоматически проверяет изменения
Swagger-документации и при необходимости **перегенерирует TypeScript-клиентов** на основе текущих API.

Он активируется **только в режиме разработки** (`ASPNETCORE_ENVIRONMENT=Development`) и использует [
`swagger-typescript-api`](https://github.com/acacode/swagger-typescript-api) для генерации.

Для правильной того, чтобы отнести тот или иной контроллер к определенному api нужно использовать
атрибут `[ApiExplorerSettings(GroupName = "users")]`

---

## ⚙️ Как работает

1. При запуске приложения таск загружает список API-конфигураций из конфигурации приложения
   `appsettings.Development.json`
2. Для каждого API:
    - Получает актуальный Swagger-документ.
    - Сравнивает его с ранее сохранённым JSON (по содержимому).
    - Если документ изменился — сохраняет новую версию и запускает генерацию TypeScript-клиента.
3. Генерация выполняется через `npx swagger-typescript-api generate` во внешнем процессе.
4. Все логи процесса (`stdout` и `stderr`) выводятся в `ILogger`.

### Важно для маршрутов контроллеров

Для сервисов, из которых генерируется TypeScript-клиент, нужно сохранять явные `action-style` маршруты в атрибутах
контроллеров, например: `get-by-object`, `get-by-id`, `create`, `update/{id}`, `delete/{id}`.

Переход на "чистые" REST-пути без таких суффиксов может ухудшить или сломать генерацию клиента и ожидаемую структуру
сгенерированных методов.

---

## ⚙️ Пример конфигурации `appsettings.Development.json`

```json
{
  "OpenApi": [
    {
      "Publish": true,
      "Name": "base",
      "Url": "/swagger/base/swagger.json",
      "Version": "v1.0.0",
      "Title": "Base service web-api",
      "Description": "Base service web-api",
      "WebClientOutputPath": "./projects/oip/src/api"
      // <- при наличии генерирует веб-клиента
    },
    {
      "Publish": true,
      "Name": "users",
      "Url": "/swagger/users/swagger.json",
      "Version": "v1.0.0",
      "Title": "User service web-api",
      "Description": "User service web-api",
      "WebClientOutputPath": "./projects/oip-common/src/user-api"
      // <- при наличии генерирует веб-клиента
    }
  ]
}
```

---

## 🧠 Ключевые аргументы генерации

При генерации таск вызывает команду:

```bash
npx swagger-typescript-api generate   -p <swagger.json>   -o <outputPath>   --unwrap-response-data   --extract-enums   --extract-responses   --extract-request-body   --extract-request-params   --modular   --module-name-first-tag   --t ./templates
```

🔹 `--t ./templates` — путь к кастомным шаблонам (при необходимости).  
🔹 `--unwrap-response-data` — извлекает полезную нагрузку из API-ответов.  
🔹 `--extract-*` флаги — выносят перечисления, типы запросов и ответов в отдельные файлы.

---

## 🧩 Папка шаблонов (`templates`)

Вы можете переопределить структуру или стиль сгенерированных файлов, добавив свои шаблоны в `Oip.WebClient/templates`.  
Формат шаблонов — тот же, что использует `swagger-typescript-api` (Handlebars, `.hbs`).

---
