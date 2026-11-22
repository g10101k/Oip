# Объектная модель доступа (Object ACL)

Object ACL позволяет выдавать права на **отдельные объекты приложения**: на завод, документ, клиента, категорию, папку
меню. Oip ничего не знает о ваших сущностях: вы регистрируете их как типы объектов, а ядро в `Oip.Base` хранит права,
наследует их по иерархии, фильтрует списки и проверяет доступ.

Пример полностью рабочего подключения — демо клиентов:

| Что                         | Где                                                                                              |
|-----------------------------|--------------------------------------------------------------------------------------------------|
| Контекст, регистрация типов | `src/Oip/Demo/TableQueryDemo/DemoCustomerTableContext.cs`                                        |
| Подключение в DI            | `src/Oip/Program.cs`                                                                             |
| Стартовые права             | `src/Oip/Extensions/WebApplicationExtensions.cs` (`SeedDemoAccessControl`)                       |
| Фильтрация и проверки в API | `src/Oip/Controllers/CustomerModuleController.cs`                                                |
| Контроллер ACL              | `src/Oip/Controllers/CustomerAccessControlController.cs`                                         |
| Фронтенд                    | `src/Oip.WebClient/projects/oip/src/app/components/customer-module/customer-module.component.ts` |

Экземпляры модулей и папки меню (`OipModuleContext`) работают на этом же механизме, см. раздел
[Безопасность модулей](#безопасность-модулей).

## Основные понятия

**Тип объекта** — строковый код с префиксом приложения: `acme.factory`, `demo.customer`. Объект однозначно задаётся
парой `ObjectRef(Type, Id)`, где `Id` — `long` (ключ `int` тоже подходит).

**Права** — строковые коды. Четыре стандартных есть у каждого типа:

| Код          | Константа                     | Смысл                         |
|--------------|-------------------------------|-------------------------------|
| `read`       | `SecurityConstants.Read`      | видеть объект                 |
| `edit`       | `SecurityConstants.Edit`      | изменять                      |
| `delete`     | `SecurityConstants.Delete`    | удалять                       |
| `manage-acl` | `SecurityConstants.ManageAcl` | читать и менять права объекта |

Свои права добавляются при регистрации типа (`.Rights("approve", "export")`). В коде, API и UI вы всегда работаете с
кодами, а хранятся права по-разному:

- четыре стандартных права — битами маски в `AclSetEntry` (`AllowMask` / `DenyMask`), это самый частый и самый быстрый
  путь;
- свои права — строками `AclSetEntryRight (AclSetId, SubjectKey, RightId, IsDeny)`, поэтому их число не ограничено.

Свой код права получает `RightId` при первой регистрации, один на весь DbContext. Поэтому одинаковый код означает
одно и то же право у всех типов контекста, и оно наследуется от родителя к потомку другого типа (например, `approve`
папки действует на документ, если у документа тоже есть `approve`). Идентификатор не меняется, поэтому **код права
нельзя переименовать** без миграции данных.

**Субъект** — кому выдаётся право. Задаётся строковым ключом, строить его лучше через `SubjectKeys`:

| Ключ                 | Построение                    | Кто это                                                                            |
|----------------------|-------------------------------|------------------------------------------------------------------------------------|
| `everyone`           | `SubjectKeys.Everyone`        | любой аутентифицированный пользователь                                             |
| `role:{имя}`         | `SubjectKeys.Role("manager")` | пользователь с ролью realm в Keycloak                                              |
| `user:{sub}`         | `SubjectKeys.User(sub)`       | конкретный пользователь, `sub` из Keycloak (не логин и не email)                   |
| `client:{client_id}` | `SubjectKeys.Client("svc")`   | сервисный аккаунт                                                                  |
| `owner`              | `SubjectKeys.Owner`           | владелец конкретного объекта (см. [Владелец](#владелец-объекта))                   |
| `group:{uuid}`       | —                             | зарезервировано; группы Keycloak пока не поддерживаются, назначайте роли на группы |

Роль `admin` (`SecurityConstants.AdminRole`) — не субъект, а **обход проверки**: администратор видит и может всё.

**Запись ACL** — `AclEntry(SubjectKey, Allow, Deny)`. Эффективные права пользователя на объект:

```
(объединение Allow всех его субъектов) минус (объединение Deny всех его субъектов)
```

Deny всегда сильнее Allow. Пример: `everyone` — `read, edit, delete`, `role:user` — deny `delete`: пользователь с ролью
`user` может читать и редактировать, но не удалять.

**Наследование.** У типа может быть родительский тип того же контекста. Новый объект наследует права родителя. На
объекте можно:

- добавить собственные записи — они **дополняют** унаследованные;
- отключить наследование (`inheritanceBroken = true`) — тогда действуют только собственные записи.

Изменение прав родителя сразу распространяется на всех потомков, кроме тех, где наследование отключено.

Права хранятся общими неизменяемыми наборами (`AclSet`): тысячи объектов с одинаковыми правами ссылаются на один набор.
Поэтому у защищаемой сущности есть колонка `AclSetId`, и фильтрация списка — это один SQL-запрос без JOIN.

## Ограничения, которые важно знать

- **Наследование работает только внутри одного DbContext.** Родитель из другого контекста или сервиса — ошибка
  конфигурации при старте.
- **Доступ к модулю и доступ к объекту проверяются вместе (И).** `[Right(...)]` по `X-Module-Instance-Id` остаётся
  «воротами»: без права на экземпляр модуля объект недоступен при любых объектных правах. Объектные права от экземпляра
  модуля не наследуются.
- Права считаются по ролям **из текущего токена**. Изменение ролей в Keycloak вступает в силу после обновления токена.
- Объекты, которых нет в ACL (например, строки, созданные до подключения), недоступны никому, кроме `admin`. См.
  [Существующие данные](#существующие-данные).

## Подключение к сервису (backend)

### 1. Сущности

Сущность, которую нужно фильтровать в списках, реализует `ISecurableEntity`. Поле `AclSetId` заполняет ядро — **никогда
не присваивайте его вручную**.

```csharp
public class Factory : ISecurableEntity
{
    public long FactoryId { get; set; }
    public long AclSetId { get; set; }
    public string? OwnerSubject { get; set; }   // нужно только для владельца
    public long SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

В конфигурации сущности добавьте индекс: `entity.HasIndex(x => x.AclSetId);`.

### 2. Контекст

Контекст сервиса реализует `IAccessControlDbContext` и добавляет таблицы ACL в **свою** схему:

```csharp
public class AcmeContext(DbContextOptions<AcmeContext> options) : DbContext(options), IAccessControlDbContext
{
    public const string SchemaName = "acme";

    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Site> Sites => Set<Site>();

    public DbSet<AclSetEntity> AclSets => Set<AclSetEntity>();
    public DbSet<AclSetEntryEntity> AclSetEntries => Set<AclSetEntryEntity>();
    public DbSet<AclSetEntryRightEntity> AclSetEntryRights => Set<AclSetEntryRightEntity>();
    public DbSet<AccessObjectEntity> AccessObjects => Set<AccessObjectEntity>();
    public DbSet<AccessObjectTypeEntity> AccessObjectTypes => Set<AccessObjectTypeEntity>();
    public DbSet<AccessRightEntity> AccessRights => Set<AccessRightEntity>();
    public DbSet<AccessObjectTypeRightEntity> AccessObjectTypeRights => Set<AccessObjectTypeRightEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyOipAccessControl(Database, SchemaName);
        // ... конфигурации ваших сущностей
    }
}
```

После этого создайте миграцию для каждого провайдера вашего сервиса:

```bash
dotnet ef migrations add AddObjectAccess_Postgres --project <ваш проект> --context <ваш контекст>
```

### 3. Регистрация типов

Удобно держать описание типов статическим методом рядом с контекстом:

```csharp
public static void ConfigureAccessControl(AccessControlOptions<AcmeContext> options)
{
    options
        .Object<Site>("acme.site", o => o
            .Id(e => e.SiteId)
            .DefaultAcl(new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read])))
        .Object<Factory>("acme.factory", o => o
            .Id(e => e.FactoryId)
            .Parent("acme.site", e => e.SiteId)
            .Owner(e => e.OwnerSubject, fillOnInsert: true)
            .Rights("approve", "export"));
}
```

Методы построителя:

| Метод                                         | Назначение                                                                                                                  |
|-----------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------|
| `Id(e => e.Key)`                              | идентификатор объекта, обязателен                                                                                           |
| `Parent("type", e => e.ParentId)`             | родитель, от которого наследуются права; `null` или `0` — корневой объект                                                   |
| `Owner(e => e.OwnerSubject, fillOnInsert)`    | свойство с `sub` владельца; `fillOnInsert: true` заполняет пустое значение текущим пользователем при создании               |
| `Rights("a", "b")` / `Right("a", "Название")` | дополнительные права типа                                                                                                   |
| `DefaultAcl(...)`                             | собственные права нового **корневого** объекта (без родителя)                                                               |
| `StoredRights()`                              | принимать права, добавленные во время работы (`Registry.EnsureRights`); нужно, когда права приходят из подключаемых модулей |

Без `DefaultAcl` новый корневой объект не доступен никому, кроме `admin`, пока права не выданы явно.

### 4. DI

```csharp
builder.Services.AddDbContext<AcmeContext>((serviceProvider, options) =>
    options.UseNpgsql(connectionString)
        .UseOipAccessControl<AcmeContext>(serviceProvider));
builder.Services.AddOipAccessControl<AcmeContext>(AcmeContext.ConfigureAccessControl);
```

Оба вызова обязательны:

- `AddOipAccessControl` регистрирует типы, сервисы и задачу старта, которая синхронизирует типы и свои права с БД (нужен
  `AddStartupRunner()`);
- `UseOipAccessControl` подключает интерцептор `SaveChanges`. Без него новые объекты не попадают в ACL.

Интерцептор в той же транзакции, что и `SaveChanges`:

- при **добавлении** регистрирует объект, проставляет `AclSetId` (права родителя или `DefaultAcl`) и владельца;
- при **изменении** родительского ключа переносит объект к новому родителю вместе с поддеревом; при изменении владельца
  обновляет его;
- при **удалении** удаляет объект из ACL. Потомки сохраняют свои права, пока их не удалят или не перенесут.

То есть в обычном коде вы просто работаете с EF: `Add`, `Remove`, смена `SiteId` — ACL поддерживается автоматически.

### 5. Контроллер ACL

Унаследуйте базовый контроллер со своим маршрутом. Клиент для него не генерируется: фронтенд обращается к нему через
`ObjectRightsService`.

```csharp
[Route("api/acme-access-control")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AcmeAccessControlController(IObjectAccessService access, ISubjectDirectory subjectDirectory)
    : BaseAccessControlController(access, subjectDirectory);
```

Методы контроллера:

| Метод                                                      | Право                                  |
|------------------------------------------------------------|----------------------------------------|
| `GET get-object-types`                                     | любой аутентифицированный              |
| `GET get-object-acl-by-object?objectType=&objectId=`       | `manage-acl`                           |
| `PUT update-object-acl`                                    | `manage-acl`                           |
| `GET get-effective-rights-by-object?objectType=&objectId=` | любой аутентифицированный (свои права) |
| `GET get-subjects-by-filter?objectType=&objectId=&filter=` | `manage-acl`                           |

`IObjectAccessService` сам находит контекст по типу объекта, поэтому один контроллер обслуживает все типы всех
зарегистрированных контекстов. `objectId` должен быть в диапазоне `1..2^53−1` (безопасное целое в JavaScript).

## Использование в коде

Инжектируйте `ObjectAccessService<TContext>` (для фильтрации и пакетных проверок) или `IObjectAccessService` (для
проверок по `ObjectRef` без привязки к контексту).

### Фильтрация списков

```csharp
var factories = await context.Factories
    .WhereAccessible(access, SecurityConstants.Read)
    .OrderBy(x => x.Name)
    .ToListAsync(cancellationToken);
```

Фильтр — один подзапрос без JOIN по сущности, на Postgres и SQL Server: для стандартного права это побитовое `&` по
`AclSetEntry`, для своего — условие по `AclSetEntryRight.RightId`. Владелец учитывается в обоих случаях. Для
`admin` фильтр не применяется. Ставьте `WhereAccessible` до пагинации, иначе страницы будут неполными.

### Проверка одного объекта

Атрибутом на действии контроллера — вместе с `[Right]` модуля:

```csharp
[HttpPut("update-factory/{id}")]
[Right(SecurityConstants.Edit)]
[ObjectRight("acme.factory", SecurityConstants.Edit)]
public async Task<IActionResult> UpdateFactory(long id, ...) { ... }
```

Идентификатор берётся из маршрута или query по имени `id`; другое имя задаётся через `RouteKey = "factoryId"`. Атрибутов
на действии может быть несколько. Ошибки возвращаются как `ApiExceptionResponse` (401 / 400 / 403).

Или в коде:

```csharp
var obj = new ObjectRef("acme.factory", id);
if (!await access.HasRightAsync(obj, "approve", cancellationToken))
    throw new ApiException("Forbidden", "The 'approve' right on the factory is required.",
        StatusCodes.Status403Forbidden);
```

`HasRightAsync` возвращает `false` для объекта, которого нет в ACL.

### Создание объекта в родителе

Право на создание дочернего объекта проверяйте на **родителе**: например, `edit` на площадке для создания завода
(`DemandCategoryEditAsync` в `CustomerModuleController`). Сам новый объект проверять не нужно: он получит права родителя
при сохранении.

### Права для строк таблицы

Чтобы фронтенд мог скрывать кнопки без запроса на каждую строку, отдайте права вместе с данными:

```csharp
string[] rowRights = [SecurityConstants.Edit, SecurityConstants.Delete, SecurityConstants.ManageAcl];
var rights = await access.GetRightsByIdsAsync<Factory>(page.Select(x => x.FactoryId).ToList(), rowRights, ct);
// rights[id] — список прав пользователя на объект
```

Выполняется один запрос на право, а не на строку. Для одного объекта — `GetEffectiveRightsAsync(obj)`.

### Изменение прав из кода

```csharp
await access.SetAclAsync(new ObjectRef("acme.site", siteId), inheritanceBroken: false,
[
    new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read]),
    new AclEntry(SubjectKeys.Role("plant-manager"), [SecurityConstants.Edit, "approve"]),
    new AclEntry(SubjectKeys.Role("auditor"), [], [SecurityConstants.Edit])
], cancellationToken);
```

`SetAclAsync` **заменяет** собственные записи объекта целиком и требует `manage-acl` у текущего пользователя. Изменения
распространяются на потомков в одной транзакции.

Варианты `SetAclUncheckedAsync` / `GetAclUncheckedAsync` не проверяют права вызывающего. Используйте их **только** в
сидинге, миграциях и фоновых задачах, никогда в обработчиках запросов пользователя.

`MoveAsync(obj, newParent)` переносит объект в другого родителя явно. Для сущностей с `Parent(...)` это делает
интерцептор, когда меняется родительский ключ.

## Владелец объекта

Владелец позволяет выдать права «создателю» объекта, не создавая отдельный набор ACL на каждый объект:

1. В сущности есть строковое свойство с `sub` владельца, в регистрации —
   `.Owner(e => e.OwnerSubject, fillOnInsert: true)`.
2. На родителе (или в `DefaultAcl`) выдаются права субъекту `owner`: `new AclEntry(SubjectKeys.Owner, [Edit, Delete])`.
3. Права `owner` действуют для пользователя, чей `sub` совпадает с `OwnerSubject` конкретного объекта.

Все клиенты категории ссылаются на один набор, но каждый пользователь редактирует только своих.

## Свои субъекты

По умолчанию субъекты берутся из claims (`ClaimsSubjectResolver`). Чтобы добавить свои ключи, например отдел из БД
приложения, зарегистрируйте `ISubjectContributor`:

```csharp
public sealed class DepartmentSubjectContributor : ISubjectContributor
{
    public IEnumerable<string> GetSubjectKeys(ClaimsPrincipal user) => ...;   // ключи вида "type:value"
}

builder.Services.AddScoped<ISubjectContributor, DepartmentSubjectContributor>();
```

Ключ должен проходить `SubjectKeys.IsValid` (тип `role`, `user`, `client` или `group`, длина до 300 символов), неверные
ключи отбрасываются. Полная замена: свой `ISubjectResolver`, зарегистрированный **до**
`AddOipAccessControl`. Поиск субъектов для редактора — `ISubjectDirectory` (по умолчанию роли и пользователи Keycloak).

## Существующие данные

Интерцептор регистрирует только объекты, созданные после подключения. Для строк, которые уже есть в БД, выполните
разовую регистрацию (родители раньше детей):

```csharp
foreach (var site in await context.Sites.AsNoTracking().ToListAsync(ct))
{
    var aclSetId = await access.RegisterAsync(new ObjectRef("acme.site", site.SiteId), null, null, ct);
    // запишите aclSetId в Site.AclSetId
}
```

`RegisterAsync` идемпотентен: для уже зарегистрированного объекта он просто возвращает текущий `AclSetId`. Готовый
образец такой миграции с переносом старых прав — `ModuleSecurityMigration`.

## Frontend (oip-common)

Из `oip-common` экспортируются `ObjectAclEditorComponent`, `HasRightDirective` и `ObjectRightsService`. Во всех местах
`apiBase` — маршрут вашего контроллера ACL без `api/`: для `api/acme-access-control` это `acme-access-control`.

### Редактор прав

```html

<oip-object-acl-editor
        apiBase="acme-access-control"
        objectType="acme.factory"
        [objectId]="factory.factoryId"
        (saved)="onRightsSaved()"/>
```

Редактор показывает унаследованные записи (только чтение), собственные записи с тремя состояниями для каждого права
(разрешено / запрещено / не задано), переключатель наследования и поиск субъектов. Если права нужно сохранять вместе с
вашей формой, передайте `[showSaveButton]="false"` и вызовите `save()` у компонента сам. Через `[rights]` можно передать
заранее загруженный список прав типа.

Открывайте редактор только при наличии `manage-acl`, иначе сервер вернёт 403.

Субъекты, которых больше нет в Keycloak (удалённая роль или пользователь), показываются как «неизвестные» с ключом.
Автоматически они не удаляются: роль, созданная заново с тем же именем, получит права обратно. Удалить такую запись
можно кнопкой в редакторе.

### Скрытие действий

Если права пришли вместе со строкой (рекомендуется для таблиц):

```html

<p-button *oipHasRight="'delete'; rights: row.rights" icon="pi pi-trash"/>
```

Если нет — директива запросит права сама (результат кэшируется на странице):

```html

<p-button
        *oipHasRight="'approve'; apiBase: 'acme-access-control'; objectType: 'acme.factory'; objectId: factory.factoryId"/>
```

`ObjectRightsService.getRights(apiBase, type, id)` даёт то же самое в коде. После изменения прав кэш сбрасывается
автоматически (`updateAcl`), вручную — `invalidate()`.

Скрытие на клиенте — только удобство. Каждое действие обязательно проверяется на сервере (`[ObjectRight]`,
`HasRightAsync`, `WhereAccessible`).

## Безопасность модулей

Экземпляры модулей и папки меню — тип `oip.module-instance` в `OipModuleContext` (`ModuleAccessControl`). Экземпляр
наследует права папки, а права, которые модуль объявляет в `GetModuleRights`, регистрируются как права этого типа.
`[Right(...)]` проверяет право на экземпляр по `X-Module-Instance-Id` через этот же механизм, а старые записи
`ModuleInstanceSecurity` один раз переносятся `ModuleSecurityMigration`: каждый экземпляр получает свои явные права с
отключённым наследованием, так что доступ не меняется. Для экземпляров модулей учитываются только `everyone`, роли и
пользователь.

## Чек-лист

1. Сущности реализуют `ISecurableEntity`, есть индекс по `AclSetId`.
2. Контекст реализует `IAccessControlDbContext` и вызывает `ApplyOipAccessControl`; миграции созданы.
3. Типы зарегистрированы через `AddOipAccessControl`, интерцептор подключён через `UseOipAccessControl`.
4. У корневых типов есть `DefaultAcl` или права выдаются явно после создания.
5. Списки фильтруются `WhereAccessible` до пагинации.
6. Действия над объектом защищены `[ObjectRight]` вместе с `[Right]`; создание проверяет право на родителе.
7. Существующие строки зарегистрированы в ACL.
8. Есть контроллер на `BaseAccessControlController`; фронтенд использует `oip-object-acl-editor` и `*oipHasRight`.
