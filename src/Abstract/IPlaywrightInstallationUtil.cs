using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Playwright.Installation.Abstract;

/// <summary>
/// A utility library for Playwright installation assurance
/// </summary>
public interface IPlaywrightInstallationUtil : IDisposable, IAsyncDisposable
{
    ValueTask EnsureInstalled(CancellationToken cancellationToken = default);
}