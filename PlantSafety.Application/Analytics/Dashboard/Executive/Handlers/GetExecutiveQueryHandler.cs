// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetExecutiveQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.Executive.Queries;
using PlantSafety.Application.Analytics.Dashboard.Executive.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.Executive.Handlers;

/// <summary>Handles <see cref="GetExecutiveQuery"/>.</summary>
public sealed class GetExecutiveQueryHandler : IRequestHandler<GetExecutiveQuery, Result<ExecutiveQueryResult>>
{
    private readonly IExecutiveRepository _repository;
    private readonly ILogger<GetExecutiveQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetExecutiveQueryHandler(IExecutiveRepository repository, ILogger<GetExecutiveQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExecutiveQueryResult>> Handle(GetExecutiveQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetExecutiveSummaryAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ExecutiveQueryResult>.Success(new ExecutiveQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Executive for Tenant {TenantId}", request.TenantId);

            return Result<ExecutiveQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.Executive.RetrievalFailed",
                "Unable to retrieve Executive data at this time.",
                Guid.NewGuid()));
        }
    }
}
