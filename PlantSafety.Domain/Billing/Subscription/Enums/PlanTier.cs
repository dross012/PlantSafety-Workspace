// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           PlanTier.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
//  SchemaVersion:  2026.1
//  Description:    Smart-enum-style value object for a subscription plan's commercial tier. Its
//                  four core Code values (Trial/Standard/Professional/Enterprise) are DELIBERATELY
//                  identical to Tenancy.Tenant.ValueObjects.SubscriptionPlanCode's own four codes --
//                  this is the additive "foreign reference" evolution that SubscriptionPlanCode's
//                  own header comment anticipated: Tenant.PlanCode.Code is unchanged and continues
//                  to be the FK a Tenant carries, and SubscriptionPlanCatalog.Resolve(SubscriptionPlanCode)
//                  looks up the matching SubscriptionPlan reference-data entry by this same Code. A
//                  fifth tier, Custom, is added for negotiated/custom enterprise contracts that are
//                  not currently selectable through the existing tenant-provisioning pipeline (no
//                  SubscriptionPlanCode.Custom exists) -- see SubscriptionPlanCatalog remarks.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.Subscription.Enums;

/// <summary>Smart-enum-style value object identifying a subscription plan's commercial tier.</summary>
public sealed class PlanTier : IEquatable<PlanTier>
{
    /// <summary>Time-limited evaluation tier. Matches <c>SubscriptionPlanCode.Trial</c>.</summary>
    public static readonly PlanTier Trial = new("Trial", "Trial");

    /// <summary>Standard commercial tier. Matches <c>SubscriptionPlanCode.Standard</c>.</summary>
    public static readonly PlanTier Standard = new("Standard", "Standard");

    /// <summary>Professional commercial tier. Matches <c>SubscriptionPlanCode.Professional</c>.</summary>
    public static readonly PlanTier Professional = new("Professional", "Professional");

    /// <summary>Enterprise commercial tier. Matches <c>SubscriptionPlanCode.Enterprise</c>.</summary>
    public static readonly PlanTier Enterprise = new("Enterprise", "Enterprise");

    /// <summary>
    /// Negotiated/custom-contract tier for bespoke enterprise agreements. Not currently reachable
    /// through <c>ProvisionTenantCommand</c> (no matching <c>SubscriptionPlanCode</c> value exists);
    /// intended for direct assignment by a platform administrator via a future dedicated command.
    /// </summary>
    public static readonly PlanTier Custom = new("Custom", "Custom");

    /// <summary>All defined PlanTier instances.</summary>
    public static readonly IReadOnlyList<PlanTier> All = [Trial, Standard, Professional, Enterprise, Custom];

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private PlanTier(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a PlanTier instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static PlanTier From(string code)
        => TryParse(code, out var value) ? value : throw new ArgumentException($"Unknown PlanTier code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a PlanTier instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out PlanTier value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <inheritdoc/>
    public bool Equals(PlanTier? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as PlanTier);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
