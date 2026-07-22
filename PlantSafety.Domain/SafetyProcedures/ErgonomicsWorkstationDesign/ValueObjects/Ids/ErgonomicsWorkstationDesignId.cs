// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ErgonomicsWorkstationDesign aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ErgonomicsWorkstationDesign aggregate root.</summary>
public readonly record struct ErgonomicsWorkstationDesignId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ErgonomicsWorkstationDesignId wrapping a non-empty GUID.</summary>
    public ErgonomicsWorkstationDesignId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ErgonomicsWorkstationDesignId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ErgonomicsWorkstationDesignId using Uuid7.</summary>
    public static ErgonomicsWorkstationDesignId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ErgonomicsWorkstationDesignId.</summary>
    public static ErgonomicsWorkstationDesignId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ErgonomicsWorkstationDesignId.</summary>
    public static ErgonomicsWorkstationDesignId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}