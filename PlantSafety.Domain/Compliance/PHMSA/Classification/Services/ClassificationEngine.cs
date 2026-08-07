// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           ClassificationEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that classifies a candidate
//                  material against a small illustrative DOT/PHMSA hazmat lookup table.
//
//                  IMPORTANT: The dictionary below is an ILLUSTRATIVE, deterministic ruleset
//                  seeded with a handful of well-known materials for demonstration/testing
//                  purposes. It is NOT a substitute for a real regulated hazmat table (e.g. the
//                  full 49 CFR 172.101 Hazardous Materials Table or a licensed hazmat database).
//                  It is intentionally structured as a simple CAS-number-keyed lookup so a real
//                  data source (a repository-backed table, an external regulatory feed, etc.)
//                  can replace the dictionary later without changing this class's public API.
//                  When no match is found, this engine returns Success=false with an explanatory
//                  message -- it never guesses or fabricates a classification.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Services;

/// <summary>Input to <see cref="ClassificationEngine.ClassifyMaterial"/>.</summary>
public sealed record ClassificationInput(
    string? CasNumber,
    string? WasteProfileCode,
    string? SdsHazardStatement,
    string MaterialName);

/// <summary>Result of <see cref="ClassificationEngine.ClassifyMaterial"/>.</summary>
public sealed record ClassificationResult(
    bool Success,
    UNNumber? UNNumber,
    ProperShippingName? ProperShippingName,
    HazardClass? HazardClass,
    PackingGroup? PackingGroup,
    IReadOnlyList<string> SpecialProvisions,
    string? ErgGuideNumber,
    bool IsMarinePollutant,
    bool IsTIH,
    TIHZone? TIHZone,
    IReadOnlyList<string> Messages)
{
    /// <summary>Convenience factory for an unsuccessful classification with a single explanatory message.</summary>
    public static ClassificationResult Failed(string message) => new(
        Success: false,
        UNNumber: null,
        ProperShippingName: null,
        HazardClass: null,
        PackingGroup: null,
        SpecialProvisions: [],
        ErgGuideNumber: null,
        IsMarinePollutant: false,
        IsTIH: false,
        TIHZone: null,
        Messages: [message]);
}

/// <summary>
/// Pure, stateless, deterministic classification rule engine for candidate hazardous materials.
/// Not a substitute for a real regulated hazmat table -- see file header for details.
/// </summary>
public sealed class ClassificationEngine : IDomainService
{
    private sealed record LookupEntry(
        string UNNumber,
        string ProperShippingName,
        HazardClass HazardClass,
        PackingGroup PackingGroup,
        IReadOnlyList<string> SpecialProvisions,
        string? ErgGuideNumber,
        bool IsMarinePollutant,
        bool IsTIH,
        TIHZone? TIHZone);

    // ---- Illustrative deterministic lookup table, keyed by CAS number. ---------------------
    // Structured to be trivially replaceable by a real data source (repository/external feed).
    private static readonly IReadOnlyDictionary<string, LookupEntry> CasNumberLookup =
        new Dictionary<string, LookupEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["67-64-1"] = new LookupEntry(   // Acetone
                UNNumber: "UN1090",
                ProperShippingName: "Acetone",
                HazardClass: Classification.ValueObjects.HazardClass.FlammableLiquid,
                PackingGroup: Enums.PackingGroup.II,
                SpecialProvisions: ["IB2", "T4", "TP1"],
                ErgGuideNumber: "127",
                IsMarinePollutant: false,
                IsTIH: false,
                TIHZone: null),

            ["7664-93-9"] = new LookupEntry( // Sulfuric acid
                UNNumber: "UN1830",
                ProperShippingName: "Sulfuric acid",
                HazardClass: Classification.ValueObjects.HazardClass.Corrosive,
                PackingGroup: Enums.PackingGroup.II,
                SpecialProvisions: ["IB2", "T8", "TP2", "TP6"],
                ErgGuideNumber: "137",
                IsMarinePollutant: false,
                IsTIH: false,
                TIHZone: null),

            ["7782-50-5"] = new LookupEntry( // Chlorine
                UNNumber: "UN1017",
                ProperShippingName: "Chlorine",
                HazardClass: Classification.ValueObjects.HazardClass.ToxicGas,
                PackingGroup: Enums.PackingGroup.NotApplicable,
                SpecialProvisions: ["1", "T50"],
                ErgGuideNumber: "124",
                IsMarinePollutant: true,
                IsTIH: true,
                TIHZone: Enums.TIHZone.B),

            ["71-43-2"] = new LookupEntry(   // Benzene
                UNNumber: "UN1114",
                ProperShippingName: "Benzene",
                HazardClass: Classification.ValueObjects.HazardClass.FlammableLiquid,
                PackingGroup: Enums.PackingGroup.II,
                SpecialProvisions: ["IB2", "T4", "TP1", "TP8"],
                ErgGuideNumber: "130",
                IsMarinePollutant: false,
                IsTIH: false,
                TIHZone: null),

            ["7664-41-7"] = new LookupEntry( // Ammonia, anhydrous
                UNNumber: "UN1005",
                ProperShippingName: "Ammonia, anhydrous",
                HazardClass: Classification.ValueObjects.HazardClass.ToxicGas,
                PackingGroup: Enums.PackingGroup.NotApplicable,
                SpecialProvisions: ["2", "T50"],
                ErgGuideNumber: "125",
                IsMarinePollutant: false,
                IsTIH: true,
                TIHZone: Enums.TIHZone.D),
        };

    /// <summary>
    /// Classifies a candidate material against the illustrative deterministic lookup table.
    /// Returns <c>Success=false</c> with an explanatory message when no match is found -- this
    /// method never guesses or fabricates a classification for an unrecognized material.
    /// </summary>
    public ClassificationResult ClassifyMaterial(ClassificationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.CasNumber))
        {
            return ClassificationResult.Failed(
                $"No CAS number supplied for material '{input.MaterialName}'; the illustrative lookup " +
                "table is keyed by CAS number and cannot classify without one.");
        }

        if (!CasNumberLookup.TryGetValue(input.CasNumber.Trim(), out var entry))
        {
            return ClassificationResult.Failed(
                $"No classification found for CAS number '{input.CasNumber}' (material '{input.MaterialName}') " +
                "in the illustrative lookup table. This is not a substitute for a full regulated hazmat table -- " +
                "consult 49 CFR 172.101 or a licensed hazmat database for an authoritative classification.");
        }

        return new ClassificationResult(
            Success: true,
            UNNumber: new UNNumber(entry.UNNumber),
            ProperShippingName: new ProperShippingName(entry.ProperShippingName),
            HazardClass: entry.HazardClass,
            PackingGroup: entry.PackingGroup,
            SpecialProvisions: entry.SpecialProvisions,
            ErgGuideNumber: entry.ErgGuideNumber,
            IsMarinePollutant: entry.IsMarinePollutant,
            IsTIH: entry.IsTIH,
            TIHZone: entry.TIHZone,
            Messages: [$"Classified '{input.MaterialName}' (CAS {input.CasNumber}) as {entry.UNNumber} / {entry.ProperShippingName}."]);
    }
}
