// ============================================================================
//  Project:        Shared.Configuration.Tests
//  File:           SecretRedactorTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Configuration.Abstractions.Manifest;
using Shared.Configuration.Abstractions.Secrets;

using Xunit;

namespace Shared.Configuration.Tests.Secrets;

public sealed class SecretRedactorTests
{
    [Fact]
    public void Redact_NeverIncludesSecretOrConnectionStringValues()
    {
        var manifest = new EnvironmentManifest(
            EnvironmentName: "Production",
            FeatureFlags: new Dictionary<string, bool> { ["EnableOpenApi"] = true, ["EnableExperimentalUi"] = false },
            ConnectionStrings: new Dictionary<string, string> { ["PlantSafetyDb"] = "Server=prod-sql;Password=TopSecret123!" },
            Secrets: new Dictionary<string, string> { ["ApiSigningKey"] = "sk_live_abcdef123456" },
            RuntimeLimits: RuntimeLimits.Default);

        RedactedEnvironmentManifest redacted = SecretRedactor.Redact(manifest);

        redacted.EnvironmentName.Should().Be("Production");
        redacted.EnabledFeatureFlags.Should().BeEquivalentTo(["EnableOpenApi"]);
        redacted.ConnectionStringKeys.Should().BeEquivalentTo(["PlantSafetyDb"]);
        redacted.SecretKeys.Should().BeEquivalentTo(["ApiSigningKey"]);

        string serialized = redacted.ToString() ?? string.Empty;
        serialized.Should().NotContain("TopSecret123!");
        serialized.Should().NotContain("sk_live_abcdef123456");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("ab", "**")]
    [InlineData("abcd", "****")]
    [InlineData("sk_live_abcdef123456", "s*******************6")]
    public void Mask_KeepsOnlyFirstAndLastCharacterForLongerValues(string input, string expected)
    {
        SecretRedactor.Mask(input).Should().Be(expected);
    }
}
