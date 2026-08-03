// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           SubscriptionPlanCode.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Minimal, self-contained plan-reference value object for the Tenant aggregate.
//                  A deliberately small, closed set of plan codes -- NOT the full Billing/
//                  Subscription domain model, which lands in a later Phase 7 pass (Billing &
//                  Subscription Management). Tenant carries only a stable plan-code reference so
//                  provisioning/feature-gating logic has something concrete to key off of today;
//                  when Billing lands, it is expected to own the authoritative plan catalog and
//                  this value object's Code becomes a foreign reference into it (additive
//                  evolution -- no breaking change to Tenant's own shape). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

/// <summary>Smart-enum-style value object referencing a tenant's subscription plan tier.</summary>
public sealed class SubscriptionPlanCode : IEquatable<SubscriptionPlanCode>
{
    /// <summary>Time-limited evaluation plan with the most restrictive default feature/module set.</summary>
    public static readonly SubscriptionPlanCode Trial = new("Trial", "Trial");

    /// <summary>Standard commercial plan.</summary>
    public static readonly SubscriptionPlanCode Standard = new("Standard", "Standard");

    /// <summary>Professional commercial plan with expanded module access.</summary>
    public static readonly SubscriptionPlanCode Professional = new("Professional", "Professional");

    /// <summary>Enterprise commercial plan with full module access and highest default limits.</summary>
    public static readonly SubscriptionPlanCode Enterprise = new("Enterprise", "Enterprise");

    /// <summary>All defined SubscriptionPlanCode instances.</summary>
    public static readonly IReadOnlyList<SubscriptionPlanCode> All = [Trial, Standard, Professional, Enterprise];

    /// <summary>The default SubscriptionPlanCode instance for newly provisioned tenants absent an explicit plan selection.</summary>
    public static SubscriptionPlanCode Default => Trial;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private SubscriptionPlanCode(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a SubscriptionPlanCode instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static SubscriptionPlanCode From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown SubscriptionPlanCode code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a SubscriptionPlanCode instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out SubscriptionPlanCode value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <inheritdoc/>
    public bool Equals(SubscriptionPlanCode? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as SubscriptionPlanCode);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
