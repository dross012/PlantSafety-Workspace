// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHealthChecksQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HealthChecks.Queries;
using PlantSafety.Application.HealthChecks.Repositories;

namespace PlantSafety.Application.HealthChecks.Handlers;

/// <summary>Handles <see cref="GetHealthChecksQuery"/>.</summary>
public sealed class GetHealthChecksQueryHandler : IRequestHandler<GetHealthChecksQuery, Result<HealthChecksQueryResult>>
{
    private readonly IHealthChecksRepository _repository;
    private readonly ILogger<GetHealthChecksQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHealthChecksQueryHandler(IHealthChecksRepository repository, ILogger<GetHealthChecksQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HealthChecksQueryResult>> Handle(GetHealthChecksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetChecksAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HealthChecksQueryResult>.Success(new HealthChecksQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HealthChecks for Tenant {TenantId}", request.TenantId);

            return Result<HealthChecksQueryResult>.Failure(new ApplicationError(
                "HealthChecks.RetrievalFailed",
                "Unable to retrieve health check data at this time.",
                Guid.NewGuid()));
        }
    }
}
