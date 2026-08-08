// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.CraneAndHoist
//  Layer:          Application
//  File:           EvaluateCraneComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate ASME B30 (crane/hoist/lifting-device) compliance for a
//                  tenant's registered crane/hoist assets by loading real
//                  PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems
//                  aggregate data via ICraneHoistSystemsRepository and running it through the
//                  Domain-layer CraneAndHoistComplianceEngine. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Evaluates ASME B30 compliance for a tenant's registered crane/hoist assets, optionally scoped to a single site.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Optional site filter. Null evaluates every site's crane/hoist assets for the tenant.</param>
public sealed record EvaluateCraneComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid? SiteId) : ICommand<Result<StandardsComplianceReportDto>>;
