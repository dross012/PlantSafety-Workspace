// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLaggingIndicatorsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.LaggingIndicators.Queries;
using PlantSafety.Application.Scorecards.LaggingIndicators.Repositories;

namespace PlantSafety.Application.Scorecards.LaggingIndicators.Handlers;

/// <summary>Handles <see cref="GetLaggingIndicatorsQuery"/>.</summary>
public sealed class GetLaggingIndicatorsQueryHandler : IRequestHandler<GetLaggingIndicatorsQuery, Result<LaggingIndicatorsQueryResult>>
{
    private readonly ILaggingIndicatorsRepository _repository;
    private readonly ILogger<GetLaggingIndicatorsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLaggingIndicatorsQueryHandler(ILaggingIndicatorsRepository repository, ILogger<GetLaggingIndicatorsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LaggingIndicatorsQueryResult>> Handle(GetLaggingIndicatorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLaggingIndicatorsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LaggingIndicatorsQueryResult>.Success(new LaggingIndicatorsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LaggingIndicators for Tenant {TenantId}", request.TenantId);

            return Result<LaggingIndicatorsQueryResult>.Failure(new ApplicationError(
                "Scorecards.LaggingIndicators.RetrievalFailed",
                "Unable to retrieve LaggingIndicators data at this time.",
                Guid.NewGuid()));
        }
    }
}