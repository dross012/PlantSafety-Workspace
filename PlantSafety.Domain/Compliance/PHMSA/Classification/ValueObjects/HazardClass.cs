// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazardClass.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object representing a DOT hazard class (1-9), optionally with a
//                  division (e.g. "1.1", "2.1", "6.1"), as defined by 49 CFR 173 Subpart D.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

/// <summary>DOT hazard class (1-9), optionally with a division (e.g. "3", "6.1").</summary>
public readonly record struct HazardClass
{
    /// <summary>Primary DOT hazard class, 1-9.</summary>
    public int Class { get; }

    /// <summary>Optional division within the class (e.g. "1" in Class 1.1). Null when not applicable.</summary>
    public string? Division { get; }

    /// <summary>Creates a HazardClass, validating the class is within 1-9.</summary>
    public HazardClass(int hazardClass, string? division = null)
    {
        if (hazardClass is < 1 or > 9)
            throw new ArgumentException("DOT hazard class must be between 1 and 9.", nameof(hazardClass));

        Class = hazardClass;
        Division = string.IsNullOrWhiteSpace(division) ? null : division.Trim();
    }

    // ---- Common DOT hazard class/division factories (49 CFR 173 Subpart D) -------------

    /// <summary>Class 1.1 -- Explosives with a mass explosion hazard.</summary>
    public static HazardClass Explosives1_1 => new(1, "1");

    /// <summary>Class 1.4 -- Explosives with a minor blast hazard.</summary>
    public static HazardClass Explosives1_4 => new(1, "4");

    /// <summary>Class 2.1 -- Flammable gas.</summary>
    public static HazardClass FlammableGas => new(2, "1");

    /// <summary>Class 2.2 -- Non-flammable, non-toxic gas.</summary>
    public static HazardClass NonFlammableGas => new(2, "2");

    /// <summary>Class 2.3 -- Toxic (poisonous) gas.</summary>
    public static HazardClass ToxicGas => new(2, "3");

    /// <summary>Class 3 -- Flammable liquid.</summary>
    public static HazardClass FlammableLiquid => new(3);

    /// <summary>Class 4.1 -- Flammable solid.</summary>
    public static HazardClass FlammableSolid => new(4, "1");

    /// <summary>Class 4.2 -- Spontaneously combustible.</summary>
    public static HazardClass SpontaneouslyCombustible => new(4, "2");

    /// <summary>Class 4.3 -- Dangerous when wet.</summary>
    public static HazardClass DangerousWhenWet => new(4, "3");

    /// <summary>Class 5.1 -- Oxidizer.</summary>
    public static HazardClass Oxidizer => new(5, "1");

    /// <summary>Class 5.2 -- Organic peroxide.</summary>
    public static HazardClass OrganicPeroxide => new(5, "2");

    /// <summary>Class 6.1 -- Toxic (poisonous) material.</summary>
    public static HazardClass ToxicMaterial => new(6, "1");

    /// <summary>Class 6.2 -- Infectious substance.</summary>
    public static HazardClass InfectiousSubstance => new(6, "2");

    /// <summary>Class 7 -- Radioactive material.</summary>
    public static HazardClass Radioactive => new(7);

    /// <summary>Class 8 -- Corrosive material.</summary>
    public static HazardClass Corrosive => new(8);

    /// <summary>Class 9 -- Miscellaneous hazardous material.</summary>
    public static HazardClass Miscellaneous => new(9);

    /// <inheritdoc/>
    public override string ToString() => Division is null ? Class.ToString() : $"{Class}.{Division}";
}
