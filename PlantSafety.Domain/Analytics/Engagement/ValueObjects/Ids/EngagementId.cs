// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Engagement aggregate root.</summary>
public readonly record struct EngagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EngagementId wrapping a non-empty GUID.</summary>
    public EngagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EngagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EngagementId using Uuid7.</summary>
    public static EngagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EngagementId.</summary>
    public static EngagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EngagementId.</summary>
    public static EngagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
