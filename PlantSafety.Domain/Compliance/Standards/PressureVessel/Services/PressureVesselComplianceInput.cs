// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.PressureVessel
//  Layer:          Domain
//  File:           PressureVesselComplianceInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input records consumed by PressureVesselComplianceEngine.EvaluateCompliance.
//                  This codebase has NO pressure-vessel domain aggregate anywhere -- these records
//                  are fully caller-supplied projections of whatever source currently tracks
//                  pressure-vessel inventory/inspection data (e.g. a manual registry, an
//                  ASME-inspection-service integration) until a dedicated aggregate exists.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.PressureVessel.Services;

/// <summary>Input to <see cref="PressureVesselComplianceEngine.EvaluateCompliance"/>.</summary>
/// <param name="Vessels">The site's current pressure vessels/boilers to evaluate.</param>
/// <param name="EvaluatedAtUtc">When this compliance evaluation is being performed.</param>
public sealed record PressureVesselComplianceInput(
    IReadOnlyList<PressureVesselRecord> Vessels,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// A single pressure vessel/boiler record for ASME BPVC evaluation. No real aggregate backs this in
/// this codebase -- fully caller-supplied.
/// </summary>
/// <param name="VesselIdentifier">Identifier of the vessel (e.g. asset tag/serial number). Must not be empty.</param>
/// <param name="VesselType">Type of vessel, e.g. "Boiler", "Air Receiver", "Storage Vessel".</param>
/// <param name="MaxAllowableWorkingPressurePsi">Maximum allowable working pressure (MAWP), in PSI, if known. Must not be negative when supplied.</param>
/// <param name="LastInspectionAtUtc">When the vessel was last inspected, if known.</param>
/// <param name="NextInspectionDueAtUtc">
/// The jurisdiction/equipment-specific inspection due date, if known. ASME BPVC itself does not
/// standardize inspection frequency -- it varies by equipment type, operating pressure, and
/// jurisdiction -- so this engine evaluates against this caller-supplied due date rather than a
/// hardcoded interval.
/// </param>
/// <param name="HasCurrentJurisdictionalCertificate">Whether the vessel currently holds a valid jurisdictional operating certificate/permit.</param>
/// <param name="NewOrReplacementInstallationPending">Whether a new or replacement installation of this vessel is currently pending.</param>
/// <param name="InstallationNoticeFiledAtUtc">When prior notice of the pending new/replacement installation was filed with the jurisdiction, if at all.</param>
public sealed record PressureVesselRecord(
    string VesselIdentifier,
    string VesselType,
    decimal? MaxAllowableWorkingPressurePsi,
    UtcTimestamp? LastInspectionAtUtc,
    UtcTimestamp? NextInspectionDueAtUtc,
    bool HasCurrentJurisdictionalCertificate,
    bool NewOrReplacementInstallationPending,
    UtcTimestamp? InstallationNoticeFiledAtUtc);
