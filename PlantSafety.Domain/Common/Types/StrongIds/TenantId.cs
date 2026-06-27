// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           TenantId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Cross-cutting StrongId for multi-tenant isolation. Every aggregate
//      that is tenant-scoped carries a TenantId on its stored-proc parameters
//      via the @TenantId convention defined in the architectural rules.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.StrongIds;

/// <summary>Cross-cutting strong-typed tenant identity. All tenant-scoped aggregates carry this.</summary>
public readonly record struct TenantId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TenantId wrapping a non-empty GUID.</summary>
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TenantId using Uuid7.</summary>
    public static TenantId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TenantId.</summary>
    public static TenantId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TenantId.</summary>
    public static TenantId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
