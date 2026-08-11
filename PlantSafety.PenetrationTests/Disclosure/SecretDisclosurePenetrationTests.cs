// ============================================================================
//  Test IDs:  PLNT-PEN-LEAK-001..005
//  Attack:    Harvesting credentials from diagnostics output (OWASP A09,
//             Security Logging and Monitoring Failures / sensitive data
//             exposure).
//
//  Surface:   An EnvironmentManifest holds the host's connection strings and
//             secrets in memory. Anything that renders it -- a diagnostics
//             endpoint, a startup log line, an exception dump -- is a
//             potential exfiltration path, and one that tends to be reachable
//             with far weaker authorization than the data the credentials
//             protect. SecretRedactor is the only sanctioned way to turn a
//             manifest into something loggable.
//
//             These tests treat the redacted output as attacker-visible and
//             assert that no secret VALUE survives into it.
// ============================================================================
using Shared.Configuration.Abstractions.Manifest;
using Shared.Configuration.Abstractions.Secrets;

namespace PlantSafety.PenetrationTests.Disclosure;

public sealed class SecretDisclosurePenetrationTests
{
    private const string SigningKeyValue = "sk-live-9f2c7b41d8e05a63";
    private const string SqlPasswordValue = "P@ssw0rd-Production-Primary";
    private const string ConnectionStringValue = "Server=prod-sql;Database=PlantSafety;User Id=svc;Password=P@ssw0rd-Production-Primary;";

    private static EnvironmentManifest BuildManifest() => new(
        EnvironmentName: "Production",
        FeatureFlags: new Dictionary<string, bool> { ["PredictiveMaintenance"] = true, ["LegacyExport"] = false },
        ConnectionStrings: new Dictionary<string, string> { ["Primary"] = ConnectionStringValue },
        Secrets: new Dictionary<string, string> { ["JwtSigningKey"] = SigningKeyValue, ["SqlPassword"] = SqlPasswordValue },
        RuntimeLimits: RuntimeLimits.Default);

    private static IEnumerable<string> AllStringsIn(RedactedEnvironmentManifest redacted) =>
        new[] { redacted.EnvironmentName }
            .Concat(redacted.EnabledFeatureFlags)
            .Concat(redacted.ConnectionStringKeys)
            .Concat(redacted.SecretKeys);

    [Fact(DisplayName = "PLNT-PEN-LEAK-001: No secret value survives redaction")]
    public void Redact_ShouldNotCarryAnySecretValue()
    {
        var redacted = SecretRedactor.Redact(BuildManifest());

        // Every string the redacted view exposes is checked against every secret, rather than
        // checking the SecretKeys list alone. A value that leaked into the WRONG field -- keys and
        // values transposed, a connection string filed under feature flags -- is still a leak, and
        // is exactly the kind of mistake a field-by-field assertion reads straight past.
        foreach (var exposed in AllStringsIn(redacted))
        {
            exposed.Should().NotContain(SigningKeyValue, "the JWT signing key must never reach a loggable view");
            exposed.Should().NotContain(SqlPasswordValue, "the SQL password must never reach a loggable view");
            exposed.Should().NotContain(ConnectionStringValue, "a connection string carries its own password inline");
        }
    }

    [Fact(DisplayName = "PLNT-PEN-LEAK-002: Secret and connection-string NAMES are preserved")]
    public void Redact_ShouldPreserveKeyNames()
    {
        var redacted = SecretRedactor.Redact(BuildManifest());

        // The counterweight to PLNT-PEN-LEAK-001. A redactor that returned empty lists would pass
        // the leak test perfectly and be useless -- an operator diagnosing a missing secret needs
        // to see WHICH secrets are configured, just never their values.
        redacted.SecretKeys.Should().BeEquivalentTo(["JwtSigningKey", "SqlPassword"]);
        redacted.ConnectionStringKeys.Should().BeEquivalentTo(["Primary"]);
        redacted.EnabledFeatureFlags.Should().BeEquivalentTo(["PredictiveMaintenance"],
            "a disabled flag is not an enabled one");
    }

    [Theory(DisplayName = "PLNT-PEN-LEAK-003: A masked secret is never returned in the clear")]
    [InlineData(SigningKeyValue)]
    [InlineData(SqlPasswordValue)]
    [InlineData("short")]
    [InlineData("abcd")]
    [InlineData("a")]
    public void Mask_ShouldNeverReturnTheOriginalValue(string secret)
    {
        SecretRedactor.Mask(secret).Should().NotBe(secret,
            "a mask that returns its input has failed silently, which is worse than not masking at all");
    }

    [Theory(DisplayName = "PLNT-PEN-LEAK-004: A short secret is masked completely, with no characters revealed")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    public void Mask_ForShortValues_ShouldRevealNothing(string secret)
    {
        var masked = SecretRedactor.Mask(secret);

        // The first/last-character hint that aids recognition of a long key would give away half
        // of a four-character value, so short values are fully starred instead. This is the case
        // where a "helpful" mask does the most damage.
        masked.Should().MatchRegex("^\\*+$", "no character of a short secret may be revealed");
        masked.Should().NotContainAny("a", "b", "c", "d");
    }

    [Fact(DisplayName = "PLNT-PEN-LEAK-005: A masked long secret reveals no interior characters")]
    public void Mask_ForLongValues_ShouldRevealOnlyTheOuterCharacters()
    {
        var masked = SecretRedactor.Mask(SigningKeyValue);

        // Reconstructing the secret from repeated observation is the threat here: each masked
        // rendering must expose the same two characters and no others, so that seeing it a
        // thousand times yields no more than seeing it once.
        masked.Should().StartWith(SigningKeyValue[0].ToString());
        masked.Should().EndWith(SigningKeyValue[^1].ToString());

        var interior = masked[1..^1];
        interior.Should().MatchRegex("^\\*+$", "everything between the first and last character must be masked");

        // NOTE (accepted, not a defect): the mask preserves the value's LENGTH, which discloses
        // how long the secret is. That is a deliberate trade-off for making truncation and
        // whitespace bugs visible in diagnostics, and it is recorded here so the disclosure is a
        // documented decision rather than an oversight. If fixed-width masking is ever preferred,
        // this assertion is where that change surfaces.
        masked.Length.Should().Be(SigningKeyValue.Length);
    }
}
