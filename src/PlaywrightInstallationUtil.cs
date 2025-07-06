using System;
using System.IO;
using Soenneker.Playwright.Installation.Abstract;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Utils.Runtime;
using Microsoft.Playwright;
using Soenneker.Utils.Directory.Abstract;
using Microsoft.Extensions.Configuration;

namespace Soenneker.Playwright.Installation;

/// <inheritdoc cref="IPlaywrightInstallationUtil"/>
public sealed class PlaywrightInstallationUtil : IPlaywrightInstallationUtil
{
    private readonly ILogger<PlaywrightInstallationUtil> _logger;
    private readonly AsyncSingleton _installer;

    public PlaywrightInstallationUtil(ILogger<PlaywrightInstallationUtil> logger, IDirectoryUtil directoryUtil, IConfiguration configuration)
    {
        _logger = logger;
        _installer = new AsyncSingleton(() =>
        {
            logger.LogDebug("⏳ Ensuring Playwright Chromium is installed...");

            string playwrightPath = GetPlaywrightPath();

            directoryUtil.CreateIfDoesNotExist(playwrightPath);

            _logger.LogInformation("Setting PLAYWRIGHT_BROWSERS_PATH to {PlaywrightPath}", playwrightPath);

            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", playwrightPath);

            bool noShell = configuration.GetValue("Playwright:NoShell", true);

            try
            {
                string[] args;

                if (noShell)
                {
                    args = ["install", "--with-deps", "--no-shell", "chromium"];
                }
                else
                {
                    args = ["install", "--with-deps", "chromium"];
                }

                int code = Program.Main(args);

                if (code != 0)
                    throw new Exception($"Playwright CLI exited with {code}");

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

    public string GetPlaywrightPath()
    {
        const string playwrightFolder = ".playwright";

        _logger.LogDebug("Resolving Playwright browser path…");

        if (RuntimeUtil.IsAzureAppService)
        {
            const string root = "/home/site/wwwroot";

            _logger.LogInformation("Detected running in Azure App Service");

            return Path.Combine(root, playwrightFolder);
        }

        return Path.Combine(AppContext.BaseDirectory, playwrightFolder);
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