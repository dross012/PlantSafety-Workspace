// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardCommunicationTrainingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Queries;
using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Repositories;

namespace PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Handlers;

/// <summary>Handles <see cref="GetHazardCommunicationTrainingQuery"/>.</summary>
public sealed class GetHazardCommunicationTrainingQueryHandler : IRequestHandler<GetHazardCommunicationTrainingQuery, Result<HazardCommunicationTrainingQueryResult>>
{
    private readonly IHazardCommunicationTrainingRepository _repository;
    private readonly ILogger<GetHazardCommunicationTrainingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardCommunicationTrainingQueryHandler(IHazardCommunicationTrainingRepository repository, ILogger<GetHazardCommunicationTrainingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationTrainingQueryResult>> Handle(GetHazardCommunicationTrainingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardCommunicationTrainingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardCommunicationTrainingQueryResult>.Success(new HazardCommunicationTrainingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardCommunicationTraining records for Tenant {TenantId}", request.TenantId);

            return Result<HazardCommunicationTrainingQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.HazardCommunicationTraining.RetrievalFailed",
                "Unable to retrieve HazardCommunicationTraining records at this time.",
                Guid.NewGuid()));
        }
    }
}