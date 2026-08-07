// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           PlacardingEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that computes the set of DOT
//                  transport placards required for a load, given the hazard classes present and
//                  whether the load is in bulk packaging.
//
//                  ILLUSTRATIVE SIMPLIFICATION NOTICE: the "Table 1 / Table 2" split and the
//                  1,001 lb aggregate-weight threshold below are a deliberately simplified,
//                  clearly-labeled illustration of the placarding-applicability logic found in
//                  49 CFR 172.504 and its Table 1 / Table 2. This is NOT a verbatim reproduction
//                  of the regulatory table and MUST NOT be treated as a compliance-certified
//                  implementation without legal/regulatory review.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;

/// <summary>Input to <see cref="PlacardingEngine.GeneratePlacards"/>.</summary>
/// <param name="Materials">The hazard classes present in the load, with quantity and unit.</param>
/// <param name="IsBulkPackaging">Whether the load is carried in bulk packaging (e.g. cargo tank, portable tank, IBC over 119 gallons).</param>
public sealed record PlacardingInput(
    IReadOnlyList<(HazardClass HazardClass, decimal Quantity, string QuantityUnit)> Materials,
    bool IsBulkPackaging);

/// <summary>Result of <see cref="PlacardingEngine.GeneratePlacards"/>.</summary>
public sealed record PlacardingResult(
    bool Success,
    IReadOnlyList<string> RequiredPlacards,
    bool AnyOrientationArrowsRequired,
    IReadOnlyList<string> Messages);

/// <summary>
/// Pure, stateless, deterministic domain service that computes required DOT transport placards.
/// See file header for the scope of the illustrative simplification applied here.
/// </summary>
public sealed class PlacardingEngine : IDomainService
{
    // ---- ILLUSTRATIVE SIMPLIFICATION of 49 CFR 172.504 Table 1 -----------------
    // Table 1 classes/divisions require placarding of any quantity, bulk or non-bulk.
    private static readonly HashSet<string> Table1HazardClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "1.1", "1.2", "1.3",   // Class 1 mass explosion / projection / fire hazard divisions
        "2.3",                  // Poison gas
        "4.3",                  // Dangerous when wet (water-reactive)
        "5.2",                  // Organic peroxide, Type B, temperature-controlled
        "6.1",                  // Poison (inhalation hazard, PG I) -- treated conservatively as Table 1 here
        "7",                    // Radioactive (Category III / Yellow-III, illustrative)
    };

    // Liquid quantity units used by the orientation-arrow heuristic below (illustrative only).
    private static readonly HashSet<string> LiquidUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        "L", "mL", "gal", "qt", "pt",
    };

    // ILLUSTRATIVE SIMPLIFICATION of the 49 CFR 172.504 Table 2 aggregate-weight threshold:
    // 1,001 lbs (454 kg) gross weight of all Table 2 materials combined triggers placarding.
    private const decimal Table2AggregateWeightThresholdLbs = 1001m;

    /// <summary>Computes the required placard set for the given load.</summary>
    public PlacardingResult GeneratePlacards(PlacardingInput input)
    {
        if (input is null || input.Materials is null || input.Materials.Count == 0)
            return new PlacardingResult(false, [], false, ["At least one material is required to compute placarding."]);

        var messages = new List<string>();
        var requiredPlacards = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        decimal table2AggregateWeightLbs = 0m;
        var table2ClassesPresent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var anyOrientationArrowsRequired = false;

        foreach (var (hazardClass, quantity, quantityUnit) in input.Materials)
        {
            var classKey = hazardClass.ToString() ?? string.Empty;

            if (Table1HazardClasses.Contains(classKey))
            {
                // Table 1 classes: placard required regardless of quantity or bulk/non-bulk.
                requiredPlacards.Add(PlacardNameFor(classKey));
                messages.Add($"Hazard class {classKey} is a Table 1 class -- placard required at any quantity.");
            }
            else
            {
                // Table 2 classes: bulk always placards; non-bulk needs the aggregate threshold.
                if (input.IsBulkPackaging)
                {
                    requiredPlacards.Add(PlacardNameFor(classKey));
                    messages.Add($"Hazard class {classKey} present in bulk packaging -- placard required.");
                }
                else
                {
                    table2ClassesPresent.Add(classKey);
                    table2AggregateWeightLbs += quantity;
                }
            }

            if (!input.IsBulkPackaging && LiquidUnits.Contains(quantityUnit ?? string.Empty))
            {
                // ILLUSTRATIVE heuristic: liquids carried in non-bulk (assumed non-rigid,
                // orientation-sensitive) packaging require orientation arrows.
                anyOrientationArrowsRequired = true;
            }
        }

        if (table2AggregateWeightLbs >= Table2AggregateWeightThresholdLbs)
        {
            foreach (var classKey in table2ClassesPresent)
                requiredPlacards.Add(PlacardNameFor(classKey));

            messages.Add(
                $"Aggregate Table 2 quantity ({table2AggregateWeightLbs} lbs) meets/exceeds the " +
                $"{Table2AggregateWeightThresholdLbs} lb threshold -- placards required for all Table 2 classes present.");
        }
        else if (table2ClassesPresent.Count > 0)
        {
            messages.Add(
                $"Aggregate Table 2 quantity ({table2AggregateWeightLbs} lbs) is below the " +
                $"{Table2AggregateWeightThresholdLbs} lb threshold -- no placard required for Table 2 classes.");
        }

        return new PlacardingResult(
            Success: true,
            RequiredPlacards: [.. requiredPlacards],
            AnyOrientationArrowsRequired: anyOrientationArrowsRequired,
            Messages: messages);
    }

    /// <summary>Maps a hazard class discriminator string to its canonical placard name (illustrative).</summary>
    private static string PlacardNameFor(string hazardClass) => hazardClass switch
    {
        "1.1" or "1.2" or "1.3" => $"EXPLOSIVES {hazardClass}",
        "2.3" => "POISON GAS",
        "3" => "FLAMMABLE 3",
        "4.1" => "FLAMMABLE SOLID 4",
        "4.2" => "SPONTANEOUSLY COMBUSTIBLE 4",
        "4.3" => "DANGEROUS WHEN WET 4",
        "5.1" => "OXIDIZER 5.1",
        "5.2" => "ORGANIC PEROXIDE 5.2",
        "6.1" => "POISON 6",
        "7" => "RADIOACTIVE 7",
        "8" => "CORROSIVE 8",
        "9" => "CLASS 9",
        _ => $"CLASS {hazardClass}",
    };
}
