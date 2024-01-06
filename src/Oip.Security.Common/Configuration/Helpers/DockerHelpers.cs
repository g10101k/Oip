using Microsoft.Extensions.Configuration;
using Oip.Security.Shared.Configuration.Configuration.Common;

namespace Oip.Security.Shared.Configuration.Helpers;

public static class DockerHelpers
{
    public static void UpdateCaCertificates()
    {
        "update-ca-certificates".Bash();
    }

    public static void ApplyDockerConfiguration(IConfiguration configuration)
    {
        var dockerConfiguration = configuration.GetSection(nameof(DockerConfiguration)).Get<DockerConfiguration>();

        if (dockerConfiguration != null && dockerConfiguration.UpdateCaCertificate) UpdateCaCertificates();
    }
}