// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.Rcra
//  Layer:          Application
//  File:           EvaluateRcraComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate RCRA (40 CFR 260-279) hazardous-waste-management
//                  compliance for a site by loading its real WasteContainer and
//                  HazardousWasteManifest read-model data and running it through the
//                  Domain-layer RcraWasteComplianceEngine. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1.
//
//                  ASSUMPTION (documented per Handbook ambiguity-resolution rule): the real
//                  Application-layer WasteContainer read repository
//                  (PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories
//                  .IWasteContainerRepository) only exposes
//                  ListContainersByAccumulationAreaAsync(tenantId, accumulationAreaId) -- there is
//                  no existing "list all containers for a site" read method anywhere in this
//                  codebase. Rather than fabricate a nonexistent repository call, this command
//                  accepts the explicit set of AccumulationAreaIds to evaluate (the caller/API
//                  request body supplies which of the site's accumulation areas are in scope),
//                  and the handler calls the real, existing per-area repository method for each.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Evaluates RCRA hazardous-waste-management compliance for a site's current waste containers and manifests.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Site whose RCRA posture is being evaluated.</param>
/// <param name="AccumulationAreaIds">
/// Identifiers of the site's accumulation areas whose containers are in scope for this evaluation
/// (see ASSUMPTION in the file header -- no site-wide container listing exists yet).
/// </param>
/// <param name="MonthlyHazardousWasteQuantityKg">Total hazardous waste generated in the evaluation calendar month, in kilograms.</param>
/// <param name="MonthlyAcuteHazardousWasteQuantityKg">Portion of the monthly quantity that is acute hazardous waste (P-listed), in kilograms.</param>
public sealed record EvaluateRcraComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid SiteId,
    IReadOnlyList<Guid> AccumulationAreaIds,
    decimal MonthlyHazardousWasteQuantityKg,
    decimal MonthlyAcuteHazardousWasteQuantityKg) : ICommand<Result<EpaComplianceReportDto>>;
