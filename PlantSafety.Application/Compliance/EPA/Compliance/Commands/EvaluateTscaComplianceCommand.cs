// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.Tsca
//  Layer:          Application
//  File:           EvaluateTscaComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate TSCA (15 U.S.C. Section2601, 40 CFR Part 700+)
//                  chemical-substance compliance for a caller-supplied chemical inventory.
//
//                  ASSUMPTION: this codebase has no persisted chemical-inventory Domain
//                  aggregate (confirmed absent -- see
//                  PlantSafety.Domain.Compliance.EPA.Tsca.Services.TscaChemicalComplianceEngine's
//                  own file header). Chemical records are therefore carried directly on this
//                  command rather than loaded from a nonexistent repository. Carries the full
//                  command envelope (CorrelationId/CausationId/CommandId/RequestedAtUtc/
//                  ApiVersionToken) per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>
/// Caller-supplied snapshot of a single chemical substance's TSCA regulatory posture, mirroring
/// the Domain engine's TscaChemicalRecord input shape.
/// </summary>
/// <param name="ChemicalName">Name/identity of the chemical substance.</param>
/// <param name="CasNumber">CAS registry number of the substance, if known.</param>
/// <param name="IsOnTscaInventory">Whether the substance is listed on the TSCA Chemical Substances Inventory.</param>
/// <param name="IsNewChemicalNotYetManufactured">Whether this is a new chemical substance not yet manufactured/imported.</param>
/// <param name="PlannedManufactureOrImportDateUtc">Planned manufacture/import date, if applicable.</param>
/// <param name="IsSubjectToSnur">Whether the substance is subject to an EPA Significant New Use Rule.</param>
/// <param name="HasSubmittedRequiredNotice">Whether the applicable PMN/SNUN has been submitted.</param>
/// <param name="NoticeSubmittedAtUtc">UTC instant the PMN/SNUN was submitted, if any.</param>
/// <param name="IsRestrictedUnderSection5f">Whether the substance is subject to an EPA Section 5(f) restriction/prohibition.</param>
public sealed record TscaChemicalRecordInput(
    string ChemicalName,
    string? CasNumber,
    bool IsOnTscaInventory,
    bool IsNewChemicalNotYetManufactured,
    DateTime? PlannedManufactureOrImportDateUtc,
    bool IsSubjectToSnur,
    bool HasSubmittedRequiredNotice,
    DateTime? NoticeSubmittedAtUtc,
    bool IsRestrictedUnderSection5f);

/// <summary>Evaluates TSCA chemical-substance compliance for a caller-supplied chemical inventory.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="Chemicals">The chemical inventory snapshot to evaluate.</param>
public sealed record EvaluateTscaComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid? SiteId,
    IReadOnlyList<TscaChemicalRecordInput> Chemicals) : ICommand<Result<EpaComplianceReportDto>>;
