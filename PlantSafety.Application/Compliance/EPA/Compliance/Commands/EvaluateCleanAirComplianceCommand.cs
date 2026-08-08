// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CleanAirAct
//  Layer:          Application
//  File:           EvaluateCleanAirComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate Clean Air Act (42 U.S.C. Section7401, 40 CFR Parts
//                  60/61/63/70) compliance for a caller-supplied set of air emission sources.
//
//                  ASSUMPTION: this codebase has no persisted emission-source Domain aggregate
//                  (confirmed absent -- see
//                  PlantSafety.Domain.Compliance.EPA.CleanAirAct.Services
//                  .CleanAirComplianceEngine's own file header). Emission-source records are
//                  therefore carried directly on this command rather than loaded from a
//                  nonexistent repository. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>
/// Caller-supplied snapshot of a single air emission source's Clean Air Act regulatory posture,
/// mirroring the Domain engine's EmissionSourceRecord input shape.
/// </summary>
/// <param name="SourceName">Name/identity of the emission source.</param>
/// <param name="AnyPollutantTonsPerYear">Highest annual emission rate for any single pollutant, in tons/year.</param>
/// <param name="SingleHapTonsPerYear">Highest annual emission rate for any single hazardous air pollutant (HAP), in tons/year.</param>
/// <param name="CombinedHapTonsPerYear">Combined annual emission rate for all hazardous air pollutants, in tons/year.</param>
/// <param name="IsSubjectToNsps">Whether the source is subject to a New Source Performance Standard (40 CFR Part 60).</param>
/// <param name="IsSubjectToNeshap">Whether the source is subject to a National Emission Standard for Hazardous Air Pollutants (40 CFR Parts 61/63).</param>
/// <param name="HasTitleVPermit">Whether the source currently holds a Title V operating permit.</param>
/// <param name="TitleVPermitExpiresAtUtc">UTC expiration date of the current Title V permit, if held.</param>
public sealed record EmissionSourceRecordInput(
    string SourceName,
    decimal AnyPollutantTonsPerYear,
    decimal SingleHapTonsPerYear,
    decimal CombinedHapTonsPerYear,
    bool IsSubjectToNsps,
    bool IsSubjectToNeshap,
    bool HasTitleVPermit,
    DateTime? TitleVPermitExpiresAtUtc);

/// <summary>Evaluates Clean Air Act compliance for a caller-supplied set of air emission sources.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EmissionSources">The emission-source snapshot to evaluate.</param>
public sealed record EvaluateCleanAirComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid? SiteId,
    IReadOnlyList<EmissionSourceRecordInput> EmissionSources) : ICommand<Result<EpaComplianceReportDto>>;
