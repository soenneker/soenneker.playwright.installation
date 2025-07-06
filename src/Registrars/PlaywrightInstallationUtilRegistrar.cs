using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Playwright.Installation.Abstract;
using Soenneker.Utils.Directory.Registrars;

namespace Soenneker.Playwright.Installation.Registrars;

/// <summary>
/// A utility library for Playwright installation assurance
/// </summary>
public static class PlaywrightInstallationUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddPlaywrightInstallationUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton().TryAddSingleton<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddPlaywrightInstallationUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped().TryAddScoped<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }
}
