# OIP
Базовый набор концепций для разработки кроссплатформенных веб-приложений основанный на следующем стеке:
* Angular 19 (prime-ng, sakai-ng);
* .NET 8.0 (EFCore);
* KeyCloak.

Основной ценностью данного проекта является подходы использованные в разработке этого примера.

# Разработка
Первый запуск:
1. Установите .NET 8.0 SDK https://dotnet.microsoft.com/en-us/download/dotnet/8.0;
2. Установите последнюю LTS версию Node.js https://nodejs.org/en;
3. Установите Docker Desktop https://www.docker.com/get-started;
4. Запустите postgres `docker compose -f docker-compose-common.yml up postgres -d`;
5. Запустите проект `Oip`, после запуска должны сгенерироваться сертификаты для текущего сайта и KeyCloack;
6. Запустите KeyCloack;
   * Для Windows: `docker compose -f docker-compose-common.yml -f docker-compose-windows.yml up keycloak -d`;
   * Для Linux/Mac: `docker compose -f docker-compose-common.yml -f docker-compose-macos-linux.yml up keycloak -d`;
7. Добавьте в KeyCloak в realm `oip` пользователя с realm ролью  `admin`. Для входа используйте логин `admin` / `P@ssw0rd`;
8. Теперь можно выполнить вход с этим пользователем на портал;

Последующие запуски можно осуществлять с помощью:
* `.devcontainer/run-unix.sh` 
* `.devcontainer/run-windows.cmd`

# Концепции

* [Модули](./Modules.md)


# Известные проблемы

1. **Использование токена в SPA** - при использовании iframe возможна утечка токена
2. **Проверка прав на resource server** - возможен доступ по прямой сслыке на ресурс.
3. **Хранение токена в LocalStorage** - Небезопасный способ хранения, делает токен уязвимым для XSS-атак.

> 🔐 **Риски**: потенциальные утечки токена, неоптимальная схема авторизации, уязвимость к взлому.