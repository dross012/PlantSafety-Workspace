// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Rcra
//  Layer:          Domain
//  File:           RcraComplianceInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input snapshot records consumed by RcraWasteComplianceEngine.EvaluateCompliance.
//                  These are plain projections of the real HazardousWasteManagement aggregates
//                  (WasteContainer, HazardousWasteManifest) -- callers (Application-layer handlers,
//                  built by a separate later effort) load the real aggregates via their repository
//                  interfaces (PlantSafety.Domain.HazardousWasteManagement.*.Interfaces.I*Repository)
//                  and project them into these snapshots. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.Rcra.ValueObjects;

/// <summary>
/// Input to <see cref="PlantSafety.Domain.Compliance.EPA.Rcra.Services.RcraWasteComplianceEngine.EvaluateCompliance"/>.
/// </summary>
/// <param name="MonthlyHazardousWasteQuantityKg">
/// Total hazardous waste generated in the evaluation calendar month, in kilograms. NOTE: quantity
/// data is not yet tracked as a domain aggregate in this codebase (WasteStream characterizes waste
/// type/hazard class but has no quantity field) -- callers must supply the monthly generated
/// quantity from whatever source currently tracks it (e.g. a manual entry, an ERP integration)
/// until a dedicated aggregate exists.
/// </param>
/// <param name="MonthlyAcuteHazardousWasteQuantityKg">
/// Portion of <paramref name="MonthlyHazardousWasteQuantityKg"/> that is acute hazardous waste
/// (P-listed under 40 CFR 261.33(e)), in kilograms. Same sourcing caveat as above.
/// </param>
public sealed record RcraComplianceInput(
    decimal MonthlyHazardousWasteQuantityKg,
    decimal MonthlyAcuteHazardousWasteQuantityKg,
    IReadOnlyList<RcraContainerSnapshot> Containers,
    IReadOnlyList<RcraManifestSnapshot> Manifests,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>Snapshot projection of a <c>WasteContainer</c> aggregate for RCRA evaluation purposes.</summary>
/// <param name="Status">Mirrors <c>WasteContainerStatus.ToString()</c> (Open/Closed/Shipped).</param>
public sealed record RcraContainerSnapshot(
    Guid WasteContainerId,
    string ContainerLabel,
    int RegulatoryTimeLimitDays,
    UtcTimestamp OpenedAtUtc,
    UtcTimestamp? ClosedAtUtc,
    string Status);

/// <summary>Snapshot projection of a <c>HazardousWasteManifest</c> aggregate for RCRA evaluation purposes.</summary>
/// <param name="Status">Mirrors <c>HazardousWasteManifestStatus.ToString()</c> (Generated/InTransit/ReceivedAtTsdf/Closed/DiscrepancyFlagged).</param>
public sealed record RcraManifestSnapshot(
    Guid ManifestId,
    string ManifestNumber,
    UtcTimestamp GeneratedAtUtc,
    UtcTimestamp? TransporterSignedAtUtc,
    UtcTimestamp? TsdfReceivedAtUtc,
    UtcTimestamp? ClosedAtUtc,
    bool IsImmutable,
    string? DiscrepancyReason,
    UtcTimestamp RetentionExpiresAtUtc,
    string Status);
