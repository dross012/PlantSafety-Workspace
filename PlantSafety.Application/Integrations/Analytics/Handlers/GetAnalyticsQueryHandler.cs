// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetAnalyticsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Analytics.Queries;
using PlantSafety.Application.Integrations.Analytics.Repositories;

namespace PlantSafety.Application.Integrations.Analytics.Handlers;

/// <summary>Handles <see cref="GetAnalyticsQuery"/>.</summary>
public sealed class GetAnalyticsQueryHandler : IRequestHandler<GetAnalyticsQuery, Result<AnalyticsQueryResult>>
{
    private readonly IAnalyticsRepository _repository;
    private readonly ILogger<GetAnalyticsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAnalyticsQueryHandler(IAnalyticsRepository repository, ILogger<GetAnalyticsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AnalyticsQueryResult>> Handle(GetAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AnalyticsQueryResult>.Success(new AnalyticsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Analytics for Tenant {TenantId}", request.TenantId);

            return Result<AnalyticsQueryResult>.Failure(new ApplicationError(
                "Integrations.Analytics.RetrievalFailed",
                "Unable to retrieve integration analytics records at this time.",
                Guid.NewGuid()));
        }
    }
}
