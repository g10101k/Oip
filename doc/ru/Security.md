# Безопасность

В текущей схеме frontend использует BFF-подход: браузер работает с backend через same-origin cookie-сессию, а токены
Keycloak остаются на серверной стороне. Это снижает риск утечки access/refresh token через XSS и убирает необходимость
хранить токены в `localStorage` или `sessionStorage`.

## Компоненты

- `BffSecurityService` во frontend отвечает за проверку сессии, вход, выход и получение CSRF-токена.
- `SecurityController` на backend публикует endpoints `/api/security/*`.
- `AddDefaultAuthentication` настраивает cookie, OpenID Connect и bearer-аутентификацию.
- `UseOipCsrfProtection` проверяет CSRF для изменяющих запросов к `/api`.
- `ITicketStore` хранит authentication ticket в Redis или in-memory хранилище.
- Keycloak остается identity provider и выполняет OIDC authentication flow.

## Аутентификация

Вход начинается с `POST /api/security/create-auth-session`. Backend создает OIDC challenge в Keycloak с authorization
code flow и PKCE. После успешной аутентификации backend сохраняет токены в серверном authentication ticket и отдает
браузеру cookie `__Host-OIP`.

Cookie настроена как `HttpOnly`, `Secure` и `SameSite=Lax`. Браузер не читает ее из JavaScript, но автоматически
передает backend для same-origin запросов.

Для проверки текущего пользователя frontend вызывает `GET /api/security/get-current-auth-session`. Ответ содержит
состояние аутентификации, имя пользователя, email и роли, но не содержит access token или refresh token.

## Авторизация

Backend защищает контроллеры через стандартные атрибуты ASP.NET Core:

- `[Authorize]` для требований аутентифицированного пользователя.
- `[Authorize(Roles = SecurityConstants.AdminRole)]` для административных операций.

Роли добавляются в claims из access token на стороне backend. Проверка прав на UI через `AuthGuardService` нужна для
удобства навигации, но не является границей безопасности. Финальное решение о доступе принимает backend.

## CSRF

Для cookie-аутентификации изменяющие запросы требуют CSRF-токен. Backend проверяет `POST`, `PUT`, `PATCH` и `DELETE`
запросы к `/api`, кроме `POST /api/security/create-auth-session`.

Поток выглядит так:

1. Frontend вызывает `GET /api/security/get-auth-csrf-token`.
2. Backend выпускает antiforgery token и возвращает имя заголовка `X-OIP-CSRF`.
3. Сгенерированный frontend `HttpClient` добавляет этот заголовок к изменяющим API-запросам.
4. Если токен отсутствует или невалиден, backend возвращает `ApiExceptionResponse` со статусом `403`.

## Logout

Выход выполняется через `POST /api/security/delete-auth-session`. Frontend отправляет скрытую POST-форму и добавляет
CSRF-токен. Backend завершает локальную cookie-сессию и OpenID Connect-сессию у Keycloak.

## Bearer-совместимость

Default authentication scheme выбирает способ проверки по запросу:

- если есть заголовок `Authorization: Bearer ...`, используется JWT bearer validation;
- если bearer-заголовка нет, используется cookie-сессия BFF.

Это позволяет оставить прямые сервисные вызовы, Swagger/API сценарии и SignalR-особенности, не возвращая хранение токенов
в браузерное приложение.

## gRPC между сервисами

gRPC-сервисы (`GrpcApplicationRegistryService`, `UserService`, `NotificationService`, `RtdsService`) слушают тот же
Kestrel endpoint, что и REST API, поэтому каждый из них закрыт политикой
`OipModuleApplication.ServiceAccountPolicy`:

```csharp
app.MapGrpcService<UserService>()
    .RequireAuthorization(OipModuleApplication.ServiceAccountPolicy);
```

Политика проверяет только JWT bearer (cookie-сессия не подходит) и пропускает токен, у которого `azp` и `client_id`
равны `SecurityService:ClientId` (`oip-backend`). Claim `client_id` Keycloak выпускает только для grant Client
Credentials, поэтому пользовательский токен, полученный через тот же клиент, отклоняется.

| Запрос                                       | Результат                     |
|----------------------------------------------|-------------------------------|
| без токена, невалидный или просроченный токен | `StatusCode.Unauthenticated`  |
| токен пользователя                           | `StatusCode.PermissionDenied` |
| токен сервисного аккаунта `oip-backend`      | вызов выполняется             |

На стороне клиента gRPC-клиент регистрируется с `AddServiceAccountAuthorization`:

```csharp
services.AddGrpcClient<GrpcUserService.GrpcUserServiceClient>(options =>
{
    options.Address = new Uri(settings.Services.UsersService);
}).AddServiceAccountAuthorization(settings);
```

`ServiceAccountTokenProvider` получает токен через OAuth 2.0 Client Credentials (`SecurityService:ClientId` и
`SecurityService:ClientSecret`), кеширует его и обновляет за 30 секунд до истечения. Если сервер ответил `401`,
закешированный токен сбрасывается и следующий вызов запросит новый. Секрет клиента задается через
`SecurityService__ClientSecret`, см. [DefaultSecrets.md](DefaultSecrets.md).

## Ошибки

Ошибки аутентификации и авторизации возвращаются в формате `ApiExceptionResponse`:

- `401 Unauthorized` - сессия отсутствует или пользователь не аутентифицирован;
- `403 Forbidden` - недостаточно прав или не прошла CSRF-проверка.

## Практические правила

- Не хранить access token и refresh token во frontend storage.
- Не добавлять ручную отправку bearer token в Angular-код для обычного SPA-потока.
- Для новых изменяющих API использовать стандартный сгенерированный `HttpClient`, чтобы CSRF-заголовок добавлялся
  автоматически.
- Проверять права на backend, даже если маршрут уже закрыт `AuthGuardService`.
- Для горизонтального масштабирования включать Redis ticket store, чтобы cookie-сессии переживали переключение между
  backend-инстансами.
