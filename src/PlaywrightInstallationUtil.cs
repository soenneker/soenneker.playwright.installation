using System;
using System.IO;
using Soenneker.Playwright.Installation.Abstract;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Enums.DeployEnvironment;
using Soenneker.Extensions.Configuration;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Extensions.String;
using Soenneker.Utils.Runtime;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Playwright.Installation;

/// <inheritdoc cref="IPlaywrightInstallationUtil"/>
public sealed class PlaywrightInstallationUtil : IPlaywrightInstallationUtil
{
    private readonly ILogger<PlaywrightInstallationUtil> _logger;
    private readonly AsyncSingleton _installer;

    public PlaywrightInstallationUtil(ILogger<PlaywrightInstallationUtil> logger, IProcessUtil processUtil, IConfiguration configuration)
    {
        _logger = logger;
        _installer = new AsyncSingleton(async (token, _) =>
        {
            logger.LogDebug("⏳ Ensuring Playwright Chromium is installed...");

            try
            {
                DeployEnvironment? environment = DeployEnvironment.FromName(configuration.GetValueStrict<string>("Environment"));

                string browserPath = await GetPlaywrightPath(token).NoSync();

                if (environment == DeployEnvironment.Local || environment == DeployEnvironment.Test)
                {
                    string baseDir = AppContext.BaseDirectory;

                    logger.LogDebug("🧪 Local environment detected. Using PowerShell script for Playwright installation.");

                    string scriptPath = Path.Combine(baseDir, "playwright.ps1");

                    await processUtil.Start("powershell", baseDir, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" install chromium",
                        cancellationToken: token).NoSync();
                }
                else
                {
                    Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browserPath);
                    logger.LogInformation("🌐 Set PLAYWRIGHT_BROWSERS_PATH to: {Path}", browserPath);
                }

                logger.LogInformation("✅ Playwright Chromium installation confirmed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to install Playwright Chromium.");
                throw;
            }

            return new object();
        });
    }

    private async ValueTask<string> GetPlaywrightPath(CancellationToken cancellationToken = default)
    {
        const string playwrightFolder = ".playwright";

        const string envVar = "PLAYWRIGHT_BROWSERS_PATH";

        _logger.LogDebug("Resolving Playwright browser path…");

        // 1️⃣ explicit override
        string? envPath = Environment.GetEnvironmentVariable(envVar);
        _logger.LogDebug("{EnvVar} = \"{Value}\"", envVar, envPath ?? "<null>");

        if (envPath.HasContent())
        {
            _logger.LogInformation("Using override from {EnvVar}: {Path}", envVar, envPath);
            return envPath;
        }

        bool container = await RuntimeUtil.IsContainer(cancellationToken);

        if (RuntimeUtil.IsAzureAppService)
        {
            const string root = "/home/site/wwwroot";

            _logger.LogInformation("Running in Azure App Service. Using {Root} as root path.", root);

            return Path.Combine(root, playwrightFolder);
        }

        // 4️⃣ fallback – local dev / any other container
        string fallback = Path.Combine(AppContext.BaseDirectory, playwrightFolder);
        _logger.LogInformation("Falling back to app base directory: {Path}", fallback);
        return fallback;
    }

    public ValueTask EnsureInstalled(CancellationToken cancellationToken = default)
    {
        return _installer.Init(cancellationToken);
    }

    public void Dispose()
    {
        _installer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _installer.DisposeAsync();
    }
}