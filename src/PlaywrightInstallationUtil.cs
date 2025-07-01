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

namespace Soenneker.Playwright.Installation;

/// <inheritdoc cref="IPlaywrightInstallationUtil"/>
public sealed class PlaywrightInstallationUtil : IPlaywrightInstallationUtil
{
    private readonly AsyncSingleton _installer;

    public PlaywrightInstallationUtil(ILogger<PlaywrightInstallationUtil> logger, IProcessUtil processUtil, IConfiguration configuration)
    {
        _installer = new AsyncSingleton(async (token, _) =>
        {
            logger.LogDebug("⏳ Ensuring Playwright Chromium is installed...");

            try
            {
                DeployEnvironment? environment = DeployEnvironment.FromName(configuration.GetValueStrict<string>("Environment"));

                string binDirectory = AppContext.BaseDirectory;

                if (environment == DeployEnvironment.Local)
                {
                    logger.LogDebug("Local environment detected, using PowerShell script for installation of Playwright.");

                    string scriptPath = Path.Combine(binDirectory, "playwright.ps1");
                    await processUtil.Start("powershell", binDirectory, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" install chromium",
                        cancellationToken: token);
                }
                else
                {
                    string browserPath = Path.Combine(binDirectory, ".playwright");

                    logger.LogInformation("Setting PLAYWRIGHT_BROWSERS_PATH ({path})...", browserPath);

                    Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browserPath);
                }

                logger.LogInformation("✅ Playwright Chromium installation confirmed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to install Playwright Chromium.");
                throw;
            }

            return new object(); // Required for AsyncSingleton<T>
        });
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