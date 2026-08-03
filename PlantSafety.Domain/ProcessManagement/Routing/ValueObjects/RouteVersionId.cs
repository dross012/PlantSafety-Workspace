// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteVersionId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the RouteVersionSnapshot aggregate
//                  root. Genuinely new: ProcessDefinition has no persisted version-snapshot
//                  concept to reuse an identity type from. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the RouteVersionSnapshot aggregate root.</summary>
public readonly record struct RouteVersionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RouteVersionId wrapping a non-empty GUID.</summary>
    public RouteVersionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RouteVersionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RouteVersionId using Uuid7.</summary>
    public static RouteVersionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RouteVersionId.</summary>
    public static RouteVersionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RouteVersionId.</summary>
    public static RouteVersionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
