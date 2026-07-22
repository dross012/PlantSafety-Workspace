// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PermitTemplate domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PermitTemplate domain events.</summary>
public readonly record struct PermitTemplateEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitTemplateEventId wrapping a non-empty GUID.</summary>
    public PermitTemplateEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitTemplateEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitTemplateEventId using Uuid7.</summary>
    public static PermitTemplateEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}