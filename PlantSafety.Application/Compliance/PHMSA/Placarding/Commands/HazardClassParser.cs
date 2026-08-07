// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           HazardClassParser.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared helper that parses a DOT hazard class discriminator string (e.g. "3",
//                  "1.1", "6.1") into the Domain-layer HazardClass value object. Used by every
//                  Placarding command handler that accepts caller-supplied hazard class strings
//                  (ApplyPlacardsCommandHandler, CalculatePlacardsCommandHandler,
//                  ValidatePlacardingForShipmentCommandHandler).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Parses DOT hazard class discriminator strings into <see cref="HazardClass"/> value objects.</summary>
public static class HazardClassParser
{
    /// <summary>
    /// Parses a hazard class string such as "3" or "1.1" into a <see cref="HazardClass"/>.
    /// Returns <see langword="false"/> (with <paramref name="hazardClass"/> defaulted) if the
    /// input is not a valid "N" or "N.D" DOT hazard class discriminator.
    /// </summary>
    public static bool TryParse(string? value, out HazardClass hazardClass)
    {
        hazardClass = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Trim().Split('.', 2);
        if (!int.TryParse(parts[0], out var classNumber))
            return false;

        var division = parts.Length > 1 ? parts[1] : null;

        try
        {
            hazardClass = new HazardClass(classNumber, division);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
