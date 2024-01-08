#pragma warning disable CS4014
#pragma warning disable S2190

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Base.Clients;
using Oip.Base.Mappers;
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
    private readonly OipClient _oipClient;
    private readonly IBaseOipModuleAppSettings _settings;
    private readonly IHostEnvironment _environment;

    /// <inheritdoc />
    public ModulesRegistryProcess(ILogger<ModulesRegistryProcess> logger, IServer server,
        OipClient oipClient, IBaseOipModuleAppSettings settings,
        IHostEnvironment environment)
    {
        _logger = logger;
        _server = server;
        _oipClient = oipClient;
        _settings = settings;
        _environment = environment;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ThrowIfNull(stoppingToken);

        Task.Run(RegisterModule, stoppingToken);
        return Task.CompletedTask;
    }

    // ReSharper disable once FunctionNeverReturns
    private async Task RegisterModule()
    {
        // Yes, this is an endless loop; if the shell crashes, the module will register again.
        while (true)
        {
            try
            {
                _logger.LogInformation($"Start register module");

                foreach (var baseUrl in GetBaseAddresses())
                {
                    var normalizeBaseUrl = NormalizeBaseUrl(baseUrl);
                    var module = _settings.ModuleFederation;
                    module.BaseUrl ??= normalizeBaseUrl;
                    module.RemoteEntry ??= $"{normalizeBaseUrl}remoteEntry.js";
                    await _oipClient.RegisterModuleAsync(new RegisterModuleDto()
                    {
                        Name = module.Name,
                        BaseUrl = module.BaseUrl,
                        RemoteEntry = module.RemoteEntry,
                        ExportModules = module.ExportModules.Select(x=>x.ToClientDto()).ToList()
                    });
                }

                _logger.LogInformation("Finish register modules ");
            }
            catch (Exception exception)
            {
                // catch all exceptions, the loop should not stop
                _logger.LogError("{exception}", exception);
            }

            Thread.Sleep(_settings.ModuleFederation.RegistryTimeOut);
        }
    }

    private IEnumerable<string> GetBaseAddresses()
    {
        var addresses = new List<string>();
        if (_environment.IsDevelopment())
        {
            addresses.Add(_settings.SpaProxyServer.ServerUrl);
            return addresses;
        }

        var addressesFeature = _server.Features.Get<IServerAddressesFeature>() ??
                               throw new InvalidOperationException(
                                   $"Cannot get {nameof(IServerAddressesFeature)}");
        addresses.AddRange(addressesFeature.Addresses);
        return addresses;
    }

    private string NormalizeBaseUrl(string url)
    {
        return url[^1] != '/' ? $"{url}/" : url;
    }
}

#pragma warning restore CS4014
#pragma warning restore S2190