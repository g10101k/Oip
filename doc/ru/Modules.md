# Описание концепции модулей в OIP
Концепция модуля в OIP заключается в создании структурированного и расширяемого подхода к управлению функциональными блоками (модулями) в приложении.

Модуль состоит из следующих элементов:
- **Контроллер (ASP.Net)** - Controller унаследованный от `BaseModuleController<TSettings>`
- **Компонент (Typescript)** - Component унаследованный от `BaseModuleComponent<TBackendStoreSettings, TLocalStoreSettings> `

Взаимосвязь между элементами **Контроллер (ASP.Net)** и **Компонент (Typescript)** задается при помощи роутинга. При инициализации приложения, происходит добавления в БД в таблицу `Module` всех модулей наследующих `BaseModuleController`, при этом также сохраняется текущий `Route` полученный через рефлексию у контроллера.

Пример правильного наименования контроллера и роутинга:
```csharp
[Route("api/weather-forecast-module")]
public class WeatherForecastModuleController : BaseModuleController<WeatherModuleSettings>
```
В Typescript роутинг задается в `app.routes.ts`
```typescript
{
  path: 'weather-forecast-module/:id',
  loadComponent: () => import('./app/components/weather-forecast-module/weather-forecast-module.component').then(m => m.WeatherForecastModuleComponent),
  canActivate: [() => inject(AuthGuardService).canActivate()]
},
```

Вот основные идеи: `BaseModuleController`
- **Абстракция:** – это абстрактный класс, который предоставляет базовую функциональность для управления модулями. Конкретные модули реализуют этот класс и предоставляют специфичные для них права доступа и настройки. `BaseModuleController`
- **Управление доступом (Security):** Модуль предоставляет механизмы для определения и управления правами доступа к данным и функциям. и позволяют получать и обновлять права доступа для конкретных экземпляров модуля. `GetSecurity``PutSecurity`
- **Настройки (Settings):** Модуль позволяет хранить и извлекать настройки, специфичные для экземпляра модуля. и обеспечивают доступ к этим настройкам. `GetModuleInstanceSettings``SaveSettings`
- **Расширяемость:** Абстрактный метод позволяет каждому модулю определять свой набор прав доступа. `GetModuleRights()`

В целом, предоставляет шаблон для создания модульных приложений с централизованным управлением доступом и настройками. Это способствует повторному использованию кода, упрощает поддержку и позволяет легко добавлять новые функциональные блоки. `BaseModuleController`