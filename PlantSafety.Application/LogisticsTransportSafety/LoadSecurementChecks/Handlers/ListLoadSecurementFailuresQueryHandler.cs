// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListLoadSecurementFailuresQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Queries;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Handlers;

/// <summary>Handles <see cref="ListLoadSecurementFailuresQuery"/>.</summary>
public sealed class ListLoadSecurementFailuresQueryHandler : IRequestHandler<ListLoadSecurementFailuresQuery, Result<LoadSecurementCheckQueryResult>>
{
    private readonly ILoadSecurementCheckRepository _repository;
    private readonly ILogger<ListLoadSecurementFailuresQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListLoadSecurementFailuresQueryHandler(ILoadSecurementCheckRepository repository, ILogger<ListLoadSecurementFailuresQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LoadSecurementCheckQueryResult>> Handle(ListLoadSecurementFailuresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListLoadSecurementFailuresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LoadSecurementCheckQueryResult>.Success(new LoadSecurementCheckQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list LoadSecurementCheck failures for Tenant {TenantId}", request.TenantId);

            return Result<LoadSecurementCheckQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.LoadSecurementChecks.RetrievalFailed",
                "Unable to list LoadSecurementCheck failures at this time.",
                Guid.NewGuid()));
        }
    }
}
