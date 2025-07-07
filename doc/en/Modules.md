# Description of the Modules Concept in OIP

The concept of a module in OIP revolves around creating a structured and extensible approach to managing functional blocks (modules) within the application.

A module consists of the following elements:
- **Controller (ASP.Net)** - A Controller inherited from `BaseModuleController<TSettings>`
- **Component (Typescript)** - A Component inherited from `BaseModuleComponent<TBackendStoreSettings, TLocalStoreSettings>`

The relationship between the **Controller (ASP.Net)** and **Component (Typescript)** is established using routing. During application initialization, all modules inheriting from `BaseModuleController` are added to the `Module` table in the database, while also saving the current `Route` obtained through reflection from the controller.

Here's an example of correct controller naming and routing:
```csharp
[Route("api/weather-forecast-module")]
public class WeatherForecastModuleController : BaseModuleController<WeatherModuleSettings>
```
In Typescript, routing is defined in `app.routes.ts`:
```typescript
{
  path: 'weather-forecast-module/:id',
  loadComponent: () => import('./app/components/weather-forecast-module/weather-forecast-module.component').then(m => m.WeatherForecastModuleComponent),
  canActivate: [() => inject(AuthGuardService).canActivate()]
},
```
Here are the core ideas of `BaseModuleController`:
- **Abstraction:** – This is an abstract class that provides basic functionality for managing modules. Specific modules implement this class and provide access and settings specific to them. `BaseModuleController`
- **Access Control (Security):** The module provides mechanisms for defining and managing access rights to data and functions, and allows obtaining and updating access rights for specific module instances. `GetSecurity`, `PutSecurity`
- **Settings:** The module allows storing and retrieving settings specific to a module instance, and provides access to those settings. `GetModuleInstanceSettings`, `SaveSettings`
- **Extensibility:** The abstract method allows each module to define its own set of access rights. `GetModuleRights()`

Overall, it provides a template for creating modular applications with centralized access control and settings management. This promotes code reuse, simplifies maintenance, and allows for easy addition of new functional blocks. `BaseModuleController`
