// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CerclaSara
//  Layer:          Application
//  File:           EvaluateCerclaReportingCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate whether a caller-supplied hazardous-substance release event
//                  triggers CERCLA Section 103 / EPCRA Section 304 emergency release-reporting
//                  obligations.
//
//                  ASSUMPTION: this codebase's SafetyEvents.IncidentManagement aggregate does not
//                  yet track substance identity/released quantity (confirmed absent -- see
//                  PlantSafety.Domain.Compliance.EPA.CerclaSara.Services
//                  .CerclaReleaseReportingEngine's own file header), so release details are
//                  carried directly on this command. LinkedIncidentManagementId is still accepted
//                  for traceability even though no lookup against it occurs in this pass. Carries
//                  the full command envelope (CorrelationId/CausationId/CommandId/RequestedAtUtc/
//                  ApiVersionToken) per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Evaluates whether a hazardous-substance release event triggers CERCLA/EPCRA emergency release-reporting obligations.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="LinkedIncidentManagementId">Optional traceability link to the real SafetyEvents.IncidentManagement aggregate this release corresponds to, if any.</param>
/// <param name="SubstanceName">Name/identity of the released substance.</param>
/// <param name="CasNumber">CAS registry number of the substance, if known.</param>
/// <param name="ReleasedQuantity">Quantity released, in <paramref name="QuantityUnit"/>.</param>
/// <param name="QuantityUnit">Unit of measure for the released/threshold quantities (e.g. "lbs", "kg").</param>
/// <param name="ReportableQuantityThreshold">The substance's own CERCLA reportable quantity (RQ) per 40 CFR 302 Table 302.4.</param>
/// <param name="IsExtremelyHazardousSubstance">EPCRA Section 302 Extremely Hazardous Substance (EHS) list membership.</param>
/// <param name="ReleaseOccurredAtUtc">UTC instant the release occurred.</param>
public sealed record EvaluateCerclaReportingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid? SiteId,
    Guid? LinkedIncidentManagementId,
    string SubstanceName,
    string? CasNumber,
    decimal ReleasedQuantity,
    string QuantityUnit,
    decimal ReportableQuantityThreshold,
    bool IsExtremelyHazardousSubstance,
    DateTime ReleaseOccurredAtUtc) : ICommand<Result<EpaComplianceReportDto>>;
