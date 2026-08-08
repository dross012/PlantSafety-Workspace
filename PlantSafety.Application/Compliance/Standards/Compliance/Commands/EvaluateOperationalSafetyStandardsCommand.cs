// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.OperationalSafety
//  Layer:          Application
//  File:           EvaluateOperationalSafetyStandardsCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate ANSI Z10 / ISO 45001 (safety management system structural
//                  elements), ANSI A1264 (walking/working surfaces), and ANSI Z117 (confined
//                  spaces) compliance for a tenant/site. This codebase has no real
//                  safety-management-system aggregate and no real walking/working-surface hazard
//                  log (confirmed absent -- see the Domain engine's own
//                  OperationalSafetyStandardsInput.cs remarks), so those two areas are
//                  caller-supplied directly on this command. Confined-space evaluation DOES
//                  consume real
//                  PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models.ConfinedSpaceItemDto
//                  data, loaded by the handler via the existing IConfinedSpaceRepository. Carries
//                  the full command envelope (CorrelationId/CausationId/CommandId/RequestedAtUtc/
//                  ApiVersionToken) per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>
/// Caller-supplied structural snapshot of an occupational health &amp; safety management system's
/// PDCA (Plan-Do-Check-Act) elements, per ANSI Z10 / ISO 45001. No real aggregate backs this in
/// this codebase -- mirrors the Domain engine's SafetyManagementSystemSnapshot input shape.
/// </summary>
public sealed record SafetyManagementSystemInput(
    bool HasDocumentedPolicy,
    bool HasPlanningProcess,
    bool HasImplementationSupport,
    bool HasPerformanceEvaluation,
    bool HasContinualImprovementProcess);

/// <summary>
/// A single caller-supplied walking/working-surface hazard record for ANSI A1264 evaluation. No
/// real aggregate backs this in this codebase -- mirrors the Domain engine's
/// WalkingWorkingSurfaceHazardRecord input shape.
/// </summary>
public sealed record WalkingWorkingSurfaceHazardInput(
    string Location,
    string HazardDescription,
    bool Corrected);

/// <summary>Evaluates ANSI Z10 / ISO 45001, ANSI A1264, and ANSI Z117 operational-safety compliance for a tenant/site.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Site whose operational-safety posture is being evaluated.</param>
/// <param name="SafetyManagementSystem">Caller-supplied safety-management-system structural snapshot, or null if not yet assessed.</param>
/// <param name="SurfaceHazards">Caller-supplied walking/working-surface hazard records.</param>
public sealed record EvaluateOperationalSafetyStandardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid SiteId,
    SafetyManagementSystemInput? SafetyManagementSystem,
    IReadOnlyList<WalkingWorkingSurfaceHazardInput> SurfaceHazards) : ICommand<Result<StandardsComplianceReportDto>>;
