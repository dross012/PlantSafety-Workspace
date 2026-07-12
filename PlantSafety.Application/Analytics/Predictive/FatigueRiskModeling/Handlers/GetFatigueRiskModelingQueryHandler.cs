// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetFatigueRiskModelingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Queries;
using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Repositories;

namespace PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Handlers;

/// <summary>Handles <see cref="GetFatigueRiskModelingQuery"/>.</summary>
public sealed class GetFatigueRiskModelingQueryHandler : IRequestHandler<GetFatigueRiskModelingQuery, Result<FatigueRiskModelingQueryResult>>
{
    private readonly IFatigueRiskModelingRepository _repository;
    private readonly ILogger<GetFatigueRiskModelingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFatigueRiskModelingQueryHandler(IFatigueRiskModelingRepository repository, ILogger<GetFatigueRiskModelingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FatigueRiskModelingQueryResult>> Handle(GetFatigueRiskModelingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetFatigueRiskAssessmentsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<FatigueRiskModelingQueryResult>.Success(new FatigueRiskModelingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FatigueRiskModeling for Tenant {TenantId}", request.TenantId);

            return Result<FatigueRiskModelingQueryResult>.Failure(new ApplicationError(
                "Analytics.Predictive.FatigueRiskModeling.RetrievalFailed",
                "Unable to retrieve FatigueRiskModeling data at this time.",
                Guid.NewGuid()));
        }
    }
}
