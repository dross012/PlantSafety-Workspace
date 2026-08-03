// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Deterministic input context supplied to every IComplianceRule invocation.
//   Carries the target entity identity plus a caller-gathered "facts" bag (already-resolved data
//   points such as permit expiry dates, MoC approval status, PPE control-measure justification
//   flags, etc.) so individual rule classes remain pure, deterministic, and independently
//   testable -- they never perform their own repository/SQL access (Handbook §2.16).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RulesEngine.Rules;

/// <summary>Deterministic input context supplied to a single IComplianceRule invocation.</summary>
/// <param name="TenantId">Tenant the target entity belongs to.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="TargetEntityType">Type discriminator of the entity being evaluated (e.g. "Risk", "Permit").</param>
/// <param name="TargetEntityId">Identity of the entity being evaluated.</param>
/// <param name="EvaluatedAtUtc">UTC instant the evaluation is running.</param>
/// <param name="Facts">Caller-gathered, already-resolved data points the rule needs to make its determination.</param>
public sealed record ComplianceRuleContext(
    Guid TenantId,
    Guid? SiteId,
    string TargetEntityType,
    Guid TargetEntityId,
    DateTime EvaluatedAtUtc,
    IReadOnlyDictionary<string, string?> Facts)
{
    /// <summary>Returns the raw fact value for <paramref name="key"/>, or <see langword="null"/> when absent.</summary>
    public string? Fact(string key) => Facts.TryGetValue(key, out var value) ? value : null;

    /// <summary>Returns the fact value for <paramref name="key"/> parsed as a UTC DateTime, or <see langword="null"/> when absent/unparsable.</summary>
    public DateTime? FactAsDateTime(string key)
        => DateTime.TryParse(Fact(key), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var value)
            ? value
            : null;

    /// <summary>Returns the fact value for <paramref name="key"/> parsed as a boolean, defaulting to <paramref name="defaultValue"/> when absent/unparsable.</summary>
    public bool FactAsBool(string key, bool defaultValue = false)
        => bool.TryParse(Fact(key), out var value) ? value : defaultValue;
}
