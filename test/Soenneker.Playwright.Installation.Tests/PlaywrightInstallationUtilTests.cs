using Soenneker.Playwright.Installation.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Playwright.Installation.Tests;

[Collection("Collection")]
public sealed class PlaywrightInstallationUtilTests : FixturedUnitTest
{
    private readonly IPlaywrightInstallationUtil _util;

    public PlaywrightInstallationUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IPlaywrightInstallationUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
