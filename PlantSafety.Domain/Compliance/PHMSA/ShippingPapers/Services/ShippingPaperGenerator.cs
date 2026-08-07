// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           ShippingPaperGenerator.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that builds the "Basic
//                  Description" sequence for a DOT shipping paper (49 CFR 172.202) and flags
//                  modal-specific additional documentation requirements.
//
//                  ILLUSTRATIVE SIMPLIFICATION NOTICE: the per-mode "additional requirement" notes
//                  produced below (IATA / IMDG / rail routing references) are illustrative flags
//                  describing that additional modal documentation exists -- they are NOT verbatim
//                  regulatory or IATA/IMO text and MUST NOT be treated as a compliance-certified
//                  implementation without legal/regulatory review.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Services;

/// <summary>Input to <see cref="ShippingPaperGenerator.BuildShippingPaper"/>.</summary>
public sealed record ShippingPaperInput(
    IReadOnlyList<(UNNumber UNNumber, ProperShippingName ShippingName, HazardClass HazardClass, PackingGroup PackingGroup)> Materials,
    TransportMode Mode,
    string? WasteManifestReference,
    string ShipperCertificationText);

/// <summary>Result of <see cref="ShippingPaperGenerator.BuildShippingPaper"/>.</summary>
public sealed record ShippingPaperResult(
    bool Success,
    ShippingPaper? ShippingPaper,
    IReadOnlyList<string> Messages);

/// <summary>
/// Pure, stateless, deterministic domain service that builds a DOT shipping paper's basic
/// description sequence and flags modal-specific additional documentation requirements.
/// </summary>
public sealed class ShippingPaperGenerator : IDomainService
{
    /// <summary>Builds a <see cref="ShippingPaper"/> from the supplied materials and transport mode.</summary>
    public ShippingPaperResult BuildShippingPaper(ShippingPaperInput input)
    {
        if (input is null || input.Materials is null || input.Materials.Count == 0)
            return new ShippingPaperResult(false, null, ["At least one material is required to build a shipping paper."]);

        var lines = new List<string>();
        foreach (var (unNumber, shippingName, hazardClass, packingGroup) in input.Materials)
        {
            lines.Add($"{unNumber}, {shippingName}, {hazardClass}, PG {packingGroup}");
        }

        var basicDescriptionSequence = string.Join(Environment.NewLine, lines);

        var additionalModalRequirements = BuildAdditionalModalRequirements(input.Mode);

        var shippingPaper = new ShippingPaper(
            BasicDescriptionSequence: basicDescriptionSequence,
            EmergencyResponseInformationRef: "See attached Emergency Response Information (49 CFR 172 Subpart G).",
            ShipperCertification: string.IsNullOrWhiteSpace(input.ShipperCertificationText)
                ? "This is to certify that the above-named materials are properly classified, described, packaged, marked, and labeled, and are in proper condition for transportation according to the applicable regulations."
                : input.ShipperCertificationText.Trim(),
            WasteManifestReference: string.IsNullOrWhiteSpace(input.WasteManifestReference) ? null : input.WasteManifestReference!.Trim(),
            AdditionalModalRequirements: additionalModalRequirements,
            GeneratedAtUtc: UtcTimestamp.Now());

        return new ShippingPaperResult(true, shippingPaper, ["Shipping paper generated successfully."]);
    }

    /// <summary>
    /// Illustrative per-mode additional-documentation flags. Not verbatim regulatory/IATA/IMO text.
    /// </summary>
    private static IReadOnlyList<string> BuildAdditionalModalRequirements(TransportMode mode) => mode switch
    {
        TransportMode.Air => ["IATA Shipper's Declaration required in addition to this shipping paper."],
        TransportMode.Vessel => ["IMDG Code declaration required."],
        TransportMode.Rail => ["Rail routing compliance per 49 CFR Part 174 required."],
        TransportMode.Ground => [],
        _ => [],
    };
}
