// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           EnvironmentManifestStartupTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.5: proves the real host actually refuses to
//                  start with invalid configuration -- not just that
//                  EnvironmentManifestValidator returns errors in
//                  isolation (already covered in Shared.Configuration.Tests),
//                  but that Program.cs's startup check genuinely stops
//                  WebApplicationFactory from building the host.
// ============================================================================
using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Shared.Configuration.Abstractions.Validation;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class EnvironmentManifestStartupTests
{
    [Fact]
    public void HostStartup_InvalidRuntimeLimits_ThrowsConfigurationValidationException()
    {
        Action act = () =>
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.UseSetting("Environment:RuntimeLimits:MaxConcurrentRequests", "0"));
            factory.CreateClient();
        };

        act.Should().Throw<ConfigurationValidationException>()
            .Which.Errors.Should().Contain(e => e.Contains("MaxConcurrentRequests"));
    }

    [Fact]
    public void HostStartup_ValidConfiguration_StartsSuccessfully()
    {
        using var factory = new WebApplicationFactory<Program>();

        Action act = () => factory.CreateClient();

        act.Should().NotThrow();
    }
}
