// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve a single ComplianceEvaluation run by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Models;

namespace PlantSafety.Application.Compliance.RulesEngine.Queries;

/// <summary>Retrieves a single ComplianceEvaluation run by identity, scoped to the tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ComplianceEvaluationId">Identity of the evaluation to retrieve.</param>
public sealed record GetComplianceEvaluationQuery(
    Guid TenantId,
    Guid ComplianceEvaluationId) : IRequest<Result<ComplianceEvaluationDto>>;
