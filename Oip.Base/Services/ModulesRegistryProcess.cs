using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Base.Api;
using Oip.Base.Settings;
using static System.ArgumentNullException;

namespace Oip.Base.Services;

/// <summary>
/// Modules Registry Process
/// </summary>
public class ModulesRegistryProcess : BackgroundService
{
    private readonly ILogger<ModulesRegistryProcess> _logger;
    private readonly IServer _server;
    private readonly ModuleFederationService _moduleFederationService;
    private readonly IBaseOipModuleAppSettings _settings;

    /// <inheritdoc />
    public ModulesRegistryProcess(ILogger<ModulesRegistryProcess> logger, IServer server,
        ModuleFederationService moduleFederationService, IBaseOipModuleAppSettings settings)
    {
        _logger = logger;
        _server = server;
        _moduleFederationService = moduleFederationService;
        _settings = settings;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ThrowIfNull(stoppingToken);

#pragma warning disable CS4014
        Task.Run(RegisterModule, stoppingToken);
        return Task.CompletedTask;
#pragma warning restore CS4014
    }

    private async Task RegisterModule()
    {
        while (true)
        {
            try
            {
                _logger.LogInformation($"Start register module {_settings.SpaProxyServer.ServerUrl}");
                var addressesFeature = _server.Features.Get<IServerAddressesFeature>() ??
                                       throw new InvalidOperationException(
                                           $"Cannot get {nameof(IServerAddressesFeature)}");
                var address = addressesFeature.Addresses.FirstOrDefault(x => x.Contains("https://")) ??
                              addressesFeature.Addresses.FirstOrDefault();
                if (address is null) throw new InvalidOperationException("Cannon find server address");

                await _moduleFederationService.RegisterModuleAsync(new RegisterModuleDto
                {
                    Name = "mfe1",
                    ModuleFederationDto = new ModuleFederationDto
                    {
                        RemoteEntry = "http://localhost:50001/remoteEntry.js",
                        BaseUrl = "http://localhost:50001/",
                        ExposedModule = "./Module",
                        DisplayName = "Flights",
                        RoutePath = "flights",
                        NgModuleName = "FlightsModule"
                    }
                });
                _logger.LogInformation("Finish register modules ");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(60000);
        }
    }
}