# Текущая схема приложения с BFF

Frontend работает с одним базовым URL и не хранит access/refresh token в браузере. Авторизационный код, обмен с Keycloak,
cookie-сессия, проверка прав и CSRF-защита находятся на backend-стороне.

```mermaid
graph TB
    subgraph "Browser"
        FE[Angular SPA<br/>AuthGuardService<br/>BffSecurityService]
        COOKIE[HttpOnly Secure cookies<br/>__Host-OIP<br/>__Host-OIP-CSRF]
    end

    subgraph "Angular CLI Proxy"
        PROXY[Dev proxy<br/>proxy.conf.js<br/>single frontend base URL]
    end

    subgraph "BFF / Main Backend"
        BFF[Oip main host<br/>https://localhost:5002]
        SECURITY[SecurityController<br/>/api/security/*]
        API[Application API<br/>/api/*]
        CSRF[CSRF middleware<br/>X-OIP-CSRF]
        TICKET[Authentication ticket store<br/>Redis or in-memory]
    end

    subgraph "Identity Provider"
        KC[Keycloak<br/>OIDC authorization code + PKCE]
    end

    subgraph "Backend Services"
        USERS[Oip.Users<br/>https://localhost:5005<br/>/api/users<br/>/api/user-profile]
        DISCUSSIONS[Oip.Discussions<br/>https://localhost:5006<br/>/api/discussion]
        NOTIFICATIONS[Oip.Notifications<br/>https://localhost:5007<br/>/api/notification<br/>/hubs/notification]
    end

    subgraph "Internal Runtime"
        GRPC[gRPC between modules/services]
        DB[(Module databases)]
    end

    FE -->|same-origin HTTP<br/>credentials: same-origin| PROXY
    FE -.->|browser stores cookies only| COOKIE
    PROXY -->|/api/security/*<br/>/api/*<br/>/swagger<br/>/health<br/>/metrics| BFF
    PROXY -->|distributed mode:<br/>/api/users<br/>/api/user-profile| USERS
    PROXY -->|distributed mode:<br/>/api/discussion| DISCUSSIONS
    PROXY -->|distributed mode:<br/>/api/notification<br/>/hubs/notification| NOTIFICATIONS

    BFF --> SECURITY
    BFF --> API
    SECURITY -->|challenge / callback / logout| KC
    SECURITY -->|stores session ticket key| TICKET
    BFF -->|validates cookie or bearer token| CSRF
    CSRF -->|protects POST/PUT/PATCH/DELETE /api| API
    API -.->|remote mode| GRPC
    USERS -.->|gRPC| GRPC
    DISCUSSIONS -.->|gRPC| GRPC
    NOTIFICATIONS -.->|gRPC + SignalR| GRPC
    API --> DB
    USERS --> DB
    DISCUSSIONS --> DB
    NOTIFICATIONS --> DB

    classDef frontend fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    classDef proxy fill:#fff8e1,stroke:#f57f17,stroke-width:2px
    classDef bff fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
    classDef identity fill:#f3e5f5,stroke:#6a1b9a,stroke-width:2px
    classDef service fill:#fce4ec,stroke:#ad1457,stroke-width:2px
    classDef data fill:#eceff1,stroke:#455a64,stroke-width:2px

    class FE,COOKIE frontend
    class PROXY proxy
    class BFF,SECURITY,API,CSRF,TICKET bff
    class KC identity
    class USERS,DISCUSSIONS,NOTIFICATIONS,GRPC service
    class DB data
```

## Основные потоки

- Вход: Angular вызывает `POST /api/security/create-auth-session`, backend запускает OIDC challenge в Keycloak и после
  успешного входа создает cookie-сессию.
- Проверка сессии: Angular вызывает `GET /api/security/get-current-auth-session` с cookie и получает только безопасные
  сведения о пользователе и ролях.
- Мутации API: сгенерированный `HttpClient` запрашивает CSRF-токен через
  `GET /api/security/get-auth-csrf-token` и отправляет его в заголовке `X-OIP-CSRF`.
- Выход: Angular отправляет POST-форму на `POST /api/security/delete-auth-session`, backend завершает cookie- и
  OpenID Connect-сессию.
- Совместимость: backend также поддерживает `Authorization: Bearer` для сценариев, где сервис вызывается напрямую или
  используется SignalR.
