using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Playwright.Installation.Abstract;
using Soenneker.Utils.Process.Registrars;

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
        services.AddProcessUtilAsSingleton().TryAddSingleton<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddPlaywrightInstallationUtilAsScoped(this IServiceCollection services)
    {
        services.AddProcessUtilAsScoped().TryAddScoped<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }
}
