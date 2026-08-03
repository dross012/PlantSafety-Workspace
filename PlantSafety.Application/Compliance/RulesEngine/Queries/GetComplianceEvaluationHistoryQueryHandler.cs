// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetComplianceEvaluationHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Compliance.RulesEngine.Repositories;

namespace PlantSafety.Application.Compliance.RulesEngine.Queries;

/// <summary>Handles <see cref="GetComplianceEvaluationHistoryQuery"/>.</summary>
public sealed class GetComplianceEvaluationHistoryQueryHandler : IRequestHandler<GetComplianceEvaluationHistoryQuery, Result<IReadOnlyList<ComplianceEvaluationDto>>>
{
    private readonly IComplianceEvaluationReadRepository _repository;
    private readonly ILogger<GetComplianceEvaluationHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceEvaluationHistoryQueryHandler(IComplianceEvaluationReadRepository repository, ILogger<GetComplianceEvaluationHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ComplianceEvaluationDto>>> Handle(GetComplianceEvaluationHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHistoryForTargetAsync(request.TenantId, request.TargetEntityType, request.TargetEntityId, cancellationToken);
            return Result<IReadOnlyList<ComplianceEvaluationDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ComplianceEvaluation history for {TargetEntityType} {TargetEntityId} Tenant {TenantId}",
                request.TargetEntityType, request.TargetEntityId, request.TenantId);

            return Result<IReadOnlyList<ComplianceEvaluationDto>>.Failure(new ApplicationError(
                "Compliance.RulesEngine.HistoryRetrievalFailed",
                "Unable to retrieve compliance evaluation history at this time.",
                Guid.NewGuid()));
        }
    }
}
