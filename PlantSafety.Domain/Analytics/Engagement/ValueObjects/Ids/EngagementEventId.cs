// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Engagement domain events.</summary>
public readonly record struct EngagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EngagementEventId wrapping a non-empty GUID.</summary>
    public EngagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EngagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EngagementEventId using Uuid7.</summary>
    public static EngagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
