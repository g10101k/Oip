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
}
