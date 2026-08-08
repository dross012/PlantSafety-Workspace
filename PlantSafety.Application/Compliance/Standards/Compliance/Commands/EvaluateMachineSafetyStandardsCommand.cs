// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.MachineSafety
//  Layer:          Application
//  File:           EvaluateMachineSafetyStandardsCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate ANSI B11 / ANSI Z244.1 / ANSI Z535 / ISO 12100 / ISO 14120 /
//                  ISO 14119 machine-safety compliance for a tenant/site by loading real
//                  MachineGuardsBarriers, LotoSystems, EmergencyStopDevices, and TwoHandControls
//                  aggregate data and running it through the Domain-layer
//                  MachineSafetyStandardsEngine. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Evaluates ANSI B11/Z244.1/Z535/ISO 12100/ISO 14120/ISO 14119 machine-safety compliance for a tenant/site's real guards, LOTO systems, e-stop devices, and two-hand controls.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Site whose machine-safety posture is being evaluated.</param>
public sealed record EvaluateMachineSafetyStandardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid SiteId) : ICommand<Result<StandardsComplianceReportDto>>;
