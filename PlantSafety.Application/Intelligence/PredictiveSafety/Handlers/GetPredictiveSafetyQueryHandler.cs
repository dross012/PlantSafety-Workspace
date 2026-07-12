// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetPredictiveSafetyQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.PredictiveSafety.Queries;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Handlers;

/// <summary>Handles <see cref="GetPredictiveSafetyQuery"/>.</summary>
public sealed class GetPredictiveSafetyQueryHandler : IRequestHandler<GetPredictiveSafetyQuery, Result<PredictiveSafetyQueryResult>>
{
    private readonly IPredictiveSafetyRepository _repository;
    private readonly ILogger<GetPredictiveSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveSafetyQueryHandler(IPredictiveSafetyRepository repository, ILogger<GetPredictiveSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveSafetyQueryResult>> Handle(GetPredictiveSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPredictionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PredictiveSafetyQueryResult>.Success(new PredictiveSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveSafety data for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveSafetyQueryResult>.Failure(new ApplicationError(
                "Intelligence.PredictiveSafety.RetrievalFailed",
                "Unable to retrieve PredictiveSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}