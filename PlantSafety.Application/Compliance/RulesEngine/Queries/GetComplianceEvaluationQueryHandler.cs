// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetComplianceEvaluationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Compliance.RulesEngine.Repositories;

namespace PlantSafety.Application.Compliance.RulesEngine.Queries;

/// <summary>Handles <see cref="GetComplianceEvaluationQuery"/>.</summary>
public sealed class GetComplianceEvaluationQueryHandler : IRequestHandler<GetComplianceEvaluationQuery, Result<ComplianceEvaluationDto>>
{
    private readonly IComplianceEvaluationReadRepository _repository;
    private readonly ILogger<GetComplianceEvaluationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceEvaluationQueryHandler(IComplianceEvaluationReadRepository repository, ILogger<GetComplianceEvaluationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceEvaluationDto>> Handle(GetComplianceEvaluationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _repository.GetByIdAsync(request.TenantId, request.ComplianceEvaluationId, cancellationToken);

            return dto is null
                ? Result<ComplianceEvaluationDto>.Failure(new ApplicationError(
                    "Compliance.RulesEngine.EvaluationNotFound",
                    "The requested compliance evaluation was not found.",
                    Guid.NewGuid()))
                : Result<ComplianceEvaluationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ComplianceEvaluation {ComplianceEvaluationId} for Tenant {TenantId}",
                request.ComplianceEvaluationId, request.TenantId);

            return Result<ComplianceEvaluationDto>.Failure(new ApplicationError(
                "Compliance.RulesEngine.RetrievalFailed",
                "Unable to retrieve the compliance evaluation at this time.",
                Guid.NewGuid()));
        }
    }
}
