// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.PressureVessel
//  Layer:          Application
//  File:           EvaluatePressureVesselComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate ASME BPVC (Boiler & Pressure Vessel Code) compliance for a
//                  caller-supplied pressure-vessel inventory. This codebase has NO pressure-vessel
//                  domain aggregate anywhere (confirmed absent -- see the Domain engine's own
//                  PressureVesselComplianceInput.cs remarks), so vessel records are carried
//                  directly on this command rather than loaded from a nonexistent repository (same
//                  pattern as Compliance.EPA.Compliance.Commands.EvaluateTscaComplianceCommand).
//                  Carries the full command envelope (CorrelationId/CausationId/CommandId/
//                  RequestedAtUtc/ApiVersionToken) per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>
/// Caller-supplied snapshot of a single pressure vessel/boiler record for ASME BPVC evaluation,
/// mirroring the Domain engine's PressureVesselRecord input shape.
/// </summary>
/// <param name="VesselIdentifier">Identifier of the vessel (e.g. asset tag/serial number). Must not be empty.</param>
/// <param name="VesselType">Type of vessel, e.g. "Boiler", "Air Receiver", "Storage Vessel".</param>
/// <param name="MaxAllowableWorkingPressurePsi">Maximum allowable working pressure (MAWP), in PSI, if known.</param>
/// <param name="LastInspectionAtUtc">When the vessel was last inspected, if known.</param>
/// <param name="NextInspectionDueAtUtc">The jurisdiction/equipment-specific inspection due date, if known.</param>
/// <param name="HasCurrentJurisdictionalCertificate">Whether the vessel currently holds a valid jurisdictional operating certificate/permit.</param>
/// <param name="NewOrReplacementInstallationPending">Whether a new or replacement installation of this vessel is currently pending.</param>
/// <param name="InstallationNoticeFiledAtUtc">When prior notice of the pending new/replacement installation was filed with the jurisdiction, if at all.</param>
public sealed record PressureVesselRecordInput(
    string VesselIdentifier,
    string VesselType,
    decimal? MaxAllowableWorkingPressurePsi,
    DateTime? LastInspectionAtUtc,
    DateTime? NextInspectionDueAtUtc,
    bool HasCurrentJurisdictionalCertificate,
    bool NewOrReplacementInstallationPending,
    DateTime? InstallationNoticeFiledAtUtc);

/// <summary>Evaluates ASME BPVC compliance for a caller-supplied pressure-vessel/boiler inventory.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="Vessels">The site's current pressure vessels/boilers to evaluate.</param>
public sealed record EvaluatePressureVesselComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    IReadOnlyList<PressureVesselRecordInput> Vessels) : ICommand<Result<StandardsComplianceReportDto>>;
