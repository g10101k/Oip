namespace Oip.Cli.Test;

public class AngularRouteEditorTests
{
    private const string Routes = """
                                  import { Routes } from '@angular/router';

                                  export const appRoutes: Routes = [
                                    {
                                      path: '',
                                      children: [
                                        {
                                          path: 'dashboard/:id',
                                          loadComponent: () => import('./app/components/dashboard/dashboard.component').then((m) => m.DashboardComponent),
                                          canActivate: [() => inject(AuthGuardService).canActivate()]
                                        },
                                        {
                                          path: 'error',
                                          loadComponent: () => import('oip-common').then((m) => m.ErrorComponent)
                                        }
                                      ]
                                    }
                                  ];
                                  """;

    [Test]
    public void InsertModuleRoute_AddsRouteToChildren()
    {
        var module = ModuleNameNormalizer.Normalize("Invoice");

        var result = AngularRouteEditor.InsertModuleRoute(Routes, module);

        Assert.That(result.Status, Is.EqualTo(RouteInsertionStatus.Inserted));
        Assert.That(result.Content, Does.Contain("path: 'invoice-module/:id'"));
        Assert.That(result.Content, Does.Contain("import('./app/components/invoice-module/invoice-module.component')"));
        Assert.That(result.Content, Does.Contain("InvoiceModuleComponent"));
    }

    [Test]
    public void InsertModuleRoute_ReturnsDuplicateForExistingRoute()
    {
        var module = ModuleNameNormalizer.Normalize("Report");

        var content = AngularRouteEditor.InsertModuleRoute(Routes, module).Content!;
        var result = AngularRouteEditor.InsertModuleRoute(content, module);

        Assert.That(result.Status, Is.EqualTo(RouteInsertionStatus.Duplicate));
    }

    [Test]
    public void InsertModuleRoute_IsIdempotentAfterInsertion()
    {
        var module = ModuleNameNormalizer.Normalize("Report");
        var first = AngularRouteEditor.InsertModuleRoute(Routes, module);

        var second = AngularRouteEditor.InsertModuleRoute(first.Content!, module);

        Assert.That(second.Status, Is.EqualTo(RouteInsertionStatus.Duplicate));
    }

    private const string ProvideOipRoutes = """
                                            import { oipAuthGuard, provideOipRoutes } from 'oip-common';

                                            export const appRoutes = provideOipRoutes({
                                              children: [
                                                {
                                                  path: 'dashboard/:id',
                                                  loadComponent: () =>
                                                    import('./app/components/dashboard/dashboard.component').then((m) => m.DashboardComponent),
                                                  canActivate: [oipAuthGuard]
                                                }
                                              ],
                                              rootRoutes: [
                                                {
                                                  path: 'smart-mirror',
                                                  loadComponent: () =>
                                                    import('./app/components/smart-mirror/smart-mirror.component').then((m) => m.SmartMirrorComponent)
                                                }
                                              ]
                                            });
                                            """;

    private const string EmptyChildren = """
                                         import { oipAuthGuard, provideOipRoutes } from 'oip-common';

                                         export const appRoutes = provideOipRoutes({
                                           children: []
                                         });
                                         """;

    [Test]
    public void InsertModuleRoute_SupportsProvideOipRoutesFormat()
    {
        var module = ModuleNameNormalizer.Normalize("GraficDashbord");

        var result = AngularRouteEditor.InsertModuleRoute(ProvideOipRoutes, module);

        Assert.That(result.Status, Is.EqualTo(RouteInsertionStatus.Inserted));
        Assert.That(result.Content, Does.Contain("path: 'grafic-dashbord-module/:id'"));
        Assert.That(result.Content, Does.Contain("canActivate: [oipAuthGuard]"));
        Assert.That(result.Content, Does.Not.Contain("AuthGuardService"));
    }

    [Test]
    public void InsertModuleRoute_KeepsRootRoutesUntouched()
    {
        var module = ModuleNameNormalizer.Normalize("GraficDashbord");

        var content = AngularRouteEditor.InsertModuleRoute(ProvideOipRoutes, module).Content!;

        var childrenBlock = content[content.IndexOf("children: [", StringComparison.Ordinal)..
            content.IndexOf("rootRoutes: [", StringComparison.Ordinal)];
        Assert.That(childrenBlock, Does.Contain("grafic-dashbord-module/:id"));
        Assert.That(content, Does.Contain("path: 'smart-mirror'"));
    }

    [Test]
    public void InsertModuleRoute_FillsEmptyChildrenArray()
    {
        var module = ModuleNameNormalizer.Normalize("Invoice");

        var result = AngularRouteEditor.InsertModuleRoute(EmptyChildren, module);

        Assert.That(result.Status, Is.EqualTo(RouteInsertionStatus.Inserted));
        Assert.That(result.Content, Does.Not.Contain("children: []"));
        Assert.That(result.Content, Does.Contain("path: 'invoice-module/:id'"));
    }

    [Test]
    public void InsertModuleRoute_ReturnsChildrenNotFoundWithoutChildrenArray()
    {
        var module = ModuleNameNormalizer.Normalize("Invoice");

        var result = AngularRouteEditor.InsertModuleRoute("export const appRoutes = [];", module);

        Assert.That(result.Status, Is.EqualTo(RouteInsertionStatus.ChildrenNotFound));
    }
}
