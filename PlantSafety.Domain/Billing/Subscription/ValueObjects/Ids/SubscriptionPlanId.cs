// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           SubscriptionPlanId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for a SubscriptionPlan reference-data entry. Deliberately
//                  NOT a random Guid per instance -- SubscriptionPlan is a small, closed catalog of
//                  reference data (mirroring SubscriptionPlanCode's own closed-set convention), so
//                  each plan's identity is deterministic (derived from its stable Code) rather than
//                  randomly generated, which keeps the catalog reproducible across environments
//                  without a seed migration. Additive-only evolution.
// ============================================================================
using System.Security.Cryptography;
using System.Text;

namespace PlantSafety.Domain.Billing.Subscription.ValueObjects.Ids;

/// <summary>Strong-typed identity for a <see cref="Aggregates.SubscriptionPlan"/> reference-data entry.</summary>
public readonly record struct SubscriptionPlanId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Wraps an existing GUID as a SubscriptionPlanId.</summary>
    public SubscriptionPlanId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionPlanId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Deterministically derives a SubscriptionPlanId from the plan's stable <paramref name="code"/>
    /// (e.g. <c>"Trial"</c>, <c>"Standard"</c>) via an MD5-based name-based GUID (RFC 4122 §4.3 style),
    /// so the same code always yields the same identity across processes/environments.
    /// </summary>
    public static SubscriptionPlanId FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("SubscriptionPlanId.FromCode requires a non-empty code.", nameof(code));

        var hash = MD5.HashData(Encoding.UTF8.GetBytes($"PlantSafety.Billing.SubscriptionPlan:{code.Trim().ToUpperInvariant()}"));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        return new SubscriptionPlanId(new Guid(bytes));
    }

    /// <summary>Wraps an existing GUID as a SubscriptionPlanId.</summary>
    public static SubscriptionPlanId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
