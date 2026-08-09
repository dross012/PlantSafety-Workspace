// ============================================================================
//  Project:        Shared.Configuration.Tests
//  File:           EnvironmentManifestLoaderTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Microsoft.Extensions.Configuration;

using Shared.Configuration.Abstractions.Manifest;

using Xunit;

namespace Shared.Configuration.Tests.Manifest;

public sealed class EnvironmentManifestLoaderTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void Load_NameAndFeatureFlagsAndConnectionStrings_ReadCorrectly()
    {
        IConfiguration config = BuildConfig(new()
        {
            ["Environment:Name"] = "Development",
            ["Environment:FeatureFlags:EnableOpenApi"] = "true",
            ["Environment:FeatureFlags:EnableExperimentalUi"] = "false",
            ["Environment:ConnectionStrings:PlantSafetyDb"] = "Server=localhost;Database=PlantSafety",
        });

        EnvironmentManifest manifest = EnvironmentManifestLoader.Load(config);

        manifest.EnvironmentName.Should().Be("Development");
        manifest.IsFeatureEnabled("EnableOpenApi").Should().BeTrue();
        manifest.IsFeatureEnabled("EnableExperimentalUi").Should().BeFalse();
        manifest.IsFeatureEnabled("NeverConfigured").Should().BeFalse();
        manifest.ConnectionStrings.Should().ContainKey("PlantSafetyDb");
    }

    [Fact]
    public void Load_NoRuntimeLimitsSection_UsesDefaults()
    {
        IConfiguration config = BuildConfig(new() { ["Environment:Name"] = "Development" });

        EnvironmentManifest manifest = EnvironmentManifestLoader.Load(config);

        manifest.RuntimeLimits.Should().Be(RuntimeLimits.Default);
    }

    [Fact]
    public void Load_RuntimeLimitsConfigured_OverridesDefaults()
    {
        IConfiguration config = BuildConfig(new()
        {
            ["Environment:Name"] = "Production",
            ["Environment:RuntimeLimits:MaxRequestBodySizeBytes"] = "1048576",
            ["Environment:RuntimeLimits:RequestTimeoutSeconds"] = "60",
            ["Environment:RuntimeLimits:MaxConcurrentRequests"] = "50",
        });

        EnvironmentManifest manifest = EnvironmentManifestLoader.Load(config);

        manifest.RuntimeLimits.MaxRequestBodySizeBytes.Should().Be(1048576);
        manifest.RuntimeLimits.RequestTimeout.Should().Be(TimeSpan.FromSeconds(60));
        manifest.RuntimeLimits.MaxConcurrentRequests.Should().Be(50);
    }

    [Fact]
    public void Load_NoEnvironmentSectionAtAll_ReturnsEmptyDefaultedManifest()
    {
        IConfiguration config = BuildConfig(new());

        EnvironmentManifest manifest = EnvironmentManifestLoader.Load(config);

        manifest.EnvironmentName.Should().BeEmpty();
        manifest.FeatureFlags.Should().BeEmpty();
        manifest.RuntimeLimits.Should().Be(RuntimeLimits.Default);
    }
}
