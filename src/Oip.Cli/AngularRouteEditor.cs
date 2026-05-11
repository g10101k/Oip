namespace Oip.Cli;

public static class AngularRouteEditor
{
    public static RouteInsertionResult InsertModuleRoute(string content, ModuleName module)
    {
        var routePath = $"{module.RoutePath}/:id";
        if (content.Contains($"path: '{routePath}'", StringComparison.Ordinal))
        {
            return RouteInsertionResult.Duplicate;
        }

        var childrenIndex = content.IndexOf("children: [", StringComparison.Ordinal);
        if (childrenIndex < 0)
        {
            return RouteInsertionResult.ChildrenNotFound;
        }

        var insertIndex = content.IndexOf("\n      {", childrenIndex, StringComparison.Ordinal);
        if (insertIndex < 0)
        {
            var emptyChildrenIndex = content.IndexOf("children: []", StringComparison.Ordinal);
            if (emptyChildrenIndex < 0)
            {
                return RouteInsertionResult.ChildrenNotFound;
            }

            var route = $"""
children: [
      {BuildRoute(module)}
    ]
""";
            return RouteInsertionResult.Inserted(content.Replace("children: []", route));
        }

        var routeBlock = Environment.NewLine + BuildRoute(module) + ",";
        return RouteInsertionResult.Inserted(content.Insert(insertIndex, routeBlock));
    }

    private static string BuildRoute(ModuleName module)
    {
        return $$"""
      {
        path: '{{module.RoutePath}}/:id',
        loadComponent: () =>
          import('./app/components/{{module.ComponentFolder}}/{{Path.GetFileNameWithoutExtension(module.ComponentFileName)}}').then(
            (m) => m.{{module.ComponentClassName}}
          ),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      }
""";
    }
}

public sealed record RouteInsertionResult(RouteInsertionStatus Status, string? Content = null)
{
    public static RouteInsertionResult Inserted(string content) => new(RouteInsertionStatus.Inserted, content);
    public static RouteInsertionResult Duplicate => new(RouteInsertionStatus.Duplicate);
    public static RouteInsertionResult ChildrenNotFound => new(RouteInsertionStatus.ChildrenNotFound);
}

public enum RouteInsertionStatus
{
    Inserted,
    Duplicate,
    ChildrenNotFound
}
