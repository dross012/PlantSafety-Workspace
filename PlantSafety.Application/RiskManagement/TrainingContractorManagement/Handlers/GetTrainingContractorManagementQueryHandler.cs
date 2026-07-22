// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetTrainingContractorManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Queries;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Repositories;

namespace PlantSafety.Application.RiskManagement.TrainingContractorManagement.Handlers;

/// <summary>Handles <see cref="GetTrainingContractorManagementQuery"/>.</summary>
public sealed class GetTrainingContractorManagementQueryHandler : IRequestHandler<GetTrainingContractorManagementQuery, Result<TrainingContractorManagementQueryResult>>
{
    private readonly ITrainingContractorManagementRepository _repository;
    private readonly ILogger<GetTrainingContractorManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrainingContractorManagementQueryHandler(ITrainingContractorManagementRepository repository, ILogger<GetTrainingContractorManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrainingContractorManagementQueryResult>> Handle(GetTrainingContractorManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrainingContractorManagementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrainingContractorManagementQueryResult>.Success(new TrainingContractorManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TrainingContractorManagement for Tenant {TenantId}", request.TenantId);

            return Result<TrainingContractorManagementQueryResult>.Failure(new ApplicationError(
                "RiskManagement.TrainingContractorManagement.RetrievalFailed",
                "Unable to retrieve TrainingContractorManagement data at this time.",
                Guid.NewGuid()));
        }
    }
}