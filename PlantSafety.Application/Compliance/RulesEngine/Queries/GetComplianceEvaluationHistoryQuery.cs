// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve the full ComplianceEvaluation history for a target entity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Models;

namespace PlantSafety.Application.Compliance.RulesEngine.Queries;

/// <summary>Retrieves the ComplianceEvaluation history for a target entity, most recent first.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="TargetEntityType">Type discriminator of the entity (e.g. "Risk", "Permit").</param>
/// <param name="TargetEntityId">Identity of the entity.</param>
public sealed record GetComplianceEvaluationHistoryQuery(
    Guid TenantId,
    string TargetEntityType,
    Guid TargetEntityId) : IRequest<Result<IReadOnlyList<ComplianceEvaluationDto>>>;
