// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the lifecycle status of a Tenant aggregate. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Tenancy.Tenant.Enums;

/// <summary>Smart Enum representing the lifecycle status of a Tenant.</summary>
public sealed class TenantStatus : IEquatable<TenantStatus>
{
    /// <summary>The tenant record has been created and the automated provisioning pipeline is running (RLS/session-context init, default reference data seed, admin user creation, role assignment, API key generation).</summary>
    public static readonly TenantStatus Provisioning = new("Provisioning", "Provisioning");

    /// <summary>The tenant is fully provisioned and operating normally.</summary>
    public static readonly TenantStatus Active = new("Active", "Active");

    /// <summary>The tenant has been temporarily suspended (e.g. billing/compliance hold); access is blocked but data is retained.</summary>
    public static readonly TenantStatus Suspended = new("Suspended", "Suspended");

    /// <summary>The tenant's data has been archived (Phase 7 §2.3 lifecycle management) and the tenant is no longer active.</summary>
    public static readonly TenantStatus Archived = new("Archived", "Archived");

    /// <summary>The tenant has been soft-deleted; retained for audit/legal-hold purposes but no longer accessible.</summary>
    public static readonly TenantStatus Deleted = new("Deleted", "Deleted");

    /// <summary>All defined TenantStatus instances.</summary>
    public static readonly IReadOnlyList<TenantStatus> All = [Provisioning, Active, Suspended, Archived, Deleted];

    /// <summary>The default TenantStatus instance.</summary>
    public static TenantStatus Default => Provisioning;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private TenantStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a TenantStatus instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static TenantStatus From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown TenantStatus code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a TenantStatus instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out TenantStatus value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(TenantStatus? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TenantStatus);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
