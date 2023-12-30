using Oip.Base;
using Oip.Base.Extensions;

namespace Oip.Module;

internal static class Program
{
  public static void Main(string[] args)
  {
    var builder = OipWebApplication.CreateBuilder(args);
    var app = builder.BuildOip();
    app.Run();
  }
}
