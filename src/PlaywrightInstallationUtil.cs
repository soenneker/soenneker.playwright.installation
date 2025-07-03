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

                string browserPath = GetPlaywrightPath();

                if (environment == DeployEnvironment.Local)
                {
                    string baseDir = AppContext.BaseDirectory;

                    logger.LogDebug("🧪 Local environment detected. Using PowerShell script for Playwright installation.");

                    string scriptPath = Path.Combine(baseDir, "playwright.ps1");

                    await processUtil.Start("powershell", baseDir, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" install chromium",
                        cancellationToken: token);
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

    private static string GetPlaywrightPath()
    {
        // 1️⃣  Respect an explicit override
        string? env = Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH");

        if (env.HasContent())
            return env;

        // 2️⃣  Custom container on Azure App Service
        if (Directory.Exists("/wwwroot"))
            return "/wwwroot/.playwright";          // same place as /home/site/wwwroot

        // 3️⃣  Built-in Linux Web App (no custom container)
        if (Directory.Exists("/home/site/wwwroot"))
            return "/home/site/wwwroot/.playwright";

        // 4️⃣  Local dev or any other container
        return Path.Combine(AppContext.BaseDirectory, ".playwright");
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