// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           ShippingPaper.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object representing a generated DOT shipping paper (49 CFR 172.202) for
//                  a hazmat shipment.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;

/// <summary>A generated DOT shipping paper for a hazmat shipment.</summary>
/// <param name="BasicDescriptionSequence">
/// The formatted "UN#, Proper Shipping Name, Hazard Class, Packing Group" line(s) per 49 CFR 172.202.
/// </param>
/// <param name="EmergencyResponseInformationRef">Reference to the accompanying emergency response information.</param>
/// <param name="ShipperCertification">Shipper's certification statement text.</param>
/// <param name="WasteManifestReference">Optional hazardous-waste manifest reference, when applicable.</param>
/// <param name="AdditionalModalRequirements">Modal-specific additional requirement notes (see ShippingPaperGenerator).</param>
/// <param name="GeneratedAtUtc">UTC instant this shipping paper was generated.</param>
public sealed record ShippingPaper(
    string BasicDescriptionSequence,
    string EmergencyResponseInformationRef,
    string ShipperCertification,
    string? WasteManifestReference,
    IReadOnlyList<string> AdditionalModalRequirements,
    UtcTimestamp GeneratedAtUtc);
