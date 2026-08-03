// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the RouteDefinition aggregate root.
//                  Mirrors PlantSafety.Domain.ProcessManagement.ValueObjects.ProcessDefinitionId.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the RouteDefinition aggregate root.</summary>
public readonly record struct RouteDefinitionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RouteDefinitionId wrapping a non-empty GUID.</summary>
    public RouteDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RouteDefinitionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RouteDefinitionId using Uuid7.</summary>
    public static RouteDefinitionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RouteDefinitionId.</summary>
    public static RouteDefinitionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RouteDefinitionId.</summary>
    public static RouteDefinitionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
