// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSupervisorQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Supervisor.Handlers;

/// <summary>Handles <see cref="GetSupervisorQuery"/>.</summary>
public sealed class GetSupervisorQueryHandler : IRequestHandler<GetSupervisorQuery, Result<SupervisorQueryResult>>
{
    private readonly ISupervisorRepository _repository;
    private readonly ILogger<GetSupervisorQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSupervisorQueryHandler(ISupervisorRepository repository, ILogger<GetSupervisorQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SupervisorQueryResult>> Handle(GetSupervisorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSupervisorScoresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SupervisorQueryResult>.Success(new SupervisorQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Supervisor for Tenant {TenantId}", request.TenantId);

            return Result<SupervisorQueryResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.Supervisor.RetrievalFailed",
                "Unable to retrieve Supervisor data at this time.",
                Guid.NewGuid()));
        }
    }
}
