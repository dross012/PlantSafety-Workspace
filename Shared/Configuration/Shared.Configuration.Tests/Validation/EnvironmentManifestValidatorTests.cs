// ============================================================================
//  Project:        Shared.Configuration.Tests
//  File:           EnvironmentManifestValidatorTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Configuration.Abstractions.Manifest;
using Shared.Configuration.Abstractions.Validation;

using Xunit;

namespace Shared.Configuration.Tests.Validation;

public sealed class EnvironmentManifestValidatorTests
{
    private static EnvironmentManifest ValidManifest() => new(
        "Development",
        new Dictionary<string, bool>(),
        new Dictionary<string, string>(),
        new Dictionary<string, string>(),
        RuntimeLimits.Default);

    [Fact]
    public void Validate_ValidManifest_ReturnsNoErrors()
    {
        EnvironmentManifestValidator.Validate(ValidManifest()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_MissingEnvironmentName_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { EnvironmentName = "" };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("Environment:Name"));
    }

    [Fact]
    public void Validate_NegativeMaxRequestBodySize_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { RuntimeLimits = RuntimeLimits.Default with { MaxRequestBodySizeBytes = -1 } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("MaxRequestBodySizeBytes"));
    }

    [Fact]
    public void Validate_ZeroRequestTimeout_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { RuntimeLimits = RuntimeLimits.Default with { RequestTimeout = TimeSpan.Zero } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("RequestTimeoutSeconds"));
    }

    [Fact]
    public void Validate_ImplausiblyLargeRequestTimeout_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { RuntimeLimits = RuntimeLimits.Default with { RequestTimeout = TimeSpan.FromHours(1) } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("implausibly large"));
    }

    [Fact]
    public void Validate_ZeroMaxConcurrentRequests_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { RuntimeLimits = RuntimeLimits.Default with { MaxConcurrentRequests = 0 } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("MaxConcurrentRequests"));
    }

    [Fact]
    public void Validate_EmptyConnectionStringValue_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { ConnectionStrings = new Dictionary<string, string> { ["PlantSafetyDb"] = "" } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("ConnectionStrings:PlantSafetyDb"));
    }

    [Fact]
    public void Validate_EmptySecretValue_ReturnsAnError()
    {
        EnvironmentManifest manifest = ValidManifest() with { Secrets = new Dictionary<string, string> { ["ApiKey"] = "" } };

        EnvironmentManifestValidator.Validate(manifest).Should().Contain(e => e.Contains("Secrets:ApiKey"));
    }

    [Fact]
    public void Validate_MultipleProblems_ReturnsAllOfThemNotJustTheFirst()
    {
        EnvironmentManifest manifest = ValidManifest() with
        {
            EnvironmentName = "",
            RuntimeLimits = RuntimeLimits.Default with { MaxRequestBodySizeBytes = 0, MaxConcurrentRequests = 0 },
        };

        EnvironmentManifestValidator.Validate(manifest).Should().HaveCount(3);
    }
}
