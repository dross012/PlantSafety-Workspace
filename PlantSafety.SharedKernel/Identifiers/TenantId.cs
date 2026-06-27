// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Identifiers
//  Layer:          SharedKernel
//  File:           TenantId.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    StrongId representing the tenant scope. Every entity and event
//                  in the platform MUST carry a TenantId. Required by multi-tenancy
//                  isolation rules. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Identifiers;

/// <summary>
/// Strong-typed tenant identifier.
/// Every aggregate, event, and API request MUST carry a <see cref="TenantId"/>.
/// </summary>
public readonly record struct TenantId
{
    public Guid Value { get; }

    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(value));
        Value = value;
    }

    public static TenantId New()               => new(Uuid7.New());
    public static TenantId FromGuid(Guid value) => new(value);
    public static TenantId Parse(string value)  => new(Guid.Parse(value));

    public override string ToString() => Value.ToString("D");
}
