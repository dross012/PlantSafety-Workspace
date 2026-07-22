// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertLocation.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Value Object representing the physical or logical location an Alert was
//                  raised at. Grounded in the source doc's "Response Instructions: ... Provide
//                  exact GPS or indoor location" (Injury & "Man Down" Alerts) and
//                  "Alert Logging & Reporting: ... Location" requirements.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.ValueObjects;

/// <summary>
/// Immutable value object describing where an <see cref="Aggregates.Alert"/> occurred: a named
/// facility zone and, optionally, precise GPS or indoor positioning coordinates.
/// </summary>
public readonly record struct AlertLocation
{
    /// <summary>Human-readable zone/area name (e.g. "Dock 4", "Confined Space B").</summary>
    public string ZoneName { get; }

    /// <summary>Optional latitude for outdoor/GPS-tracked alerts.</summary>
    public double? Latitude { get; }

    /// <summary>Optional longitude for outdoor/GPS-tracked alerts.</summary>
    public double? Longitude { get; }

    /// <summary>Optional indoor positioning reference (e.g. beacon or map coordinate id).</summary>
    public string? IndoorPositionReference { get; }

    private AlertLocation(string zoneName, double? latitude, double? longitude, string? indoorPositionReference)
    {
        ZoneName = zoneName;
        Latitude = latitude;
        Longitude = longitude;
        IndoorPositionReference = indoorPositionReference;
    }

    /// <summary>Creates a normalized <see cref="AlertLocation"/>. Rejects null/empty/whitespace zone names.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="zoneName"/> is null, empty, or whitespace.</exception>
    public static AlertLocation Create(
        string zoneName,
        double? latitude = null,
        double? longitude = null,
        string? indoorPositionReference = null)
    {
        if (string.IsNullOrWhiteSpace(zoneName))
            throw new ArgumentException("AlertLocation.ZoneName must not be null or whitespace.", nameof(zoneName));

        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

        return new AlertLocation(
            zoneName.Trim().Replace("\r", "").Replace("\n", " "),
            latitude,
            longitude,
            string.IsNullOrWhiteSpace(indoorPositionReference) ? null : indoorPositionReference.Trim());
    }

    /// <summary>Sentinel value for an unknown/unspecified location.</summary>
    public static readonly AlertLocation Unknown = new("Unspecified", null, null, null);

    /// <inheritdoc/>
    public override string ToString() => ZoneName;
}
