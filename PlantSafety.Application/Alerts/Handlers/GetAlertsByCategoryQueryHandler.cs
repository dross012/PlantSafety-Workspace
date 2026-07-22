// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetAlertsByCategoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Handlers;

/// <summary>Handles <see cref="GetAlertsByCategoryQuery"/>.</summary>
public sealed class GetAlertsByCategoryQueryHandler : IRequestHandler<GetAlertsByCategoryQuery, Result<GetAlertsByCategoryResult>>
{
    private readonly IAlertQueryRepository _repository;
    private readonly ILogger<GetAlertsByCategoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAlertsByCategoryQueryHandler(IAlertQueryRepository repository, ILogger<GetAlertsByCategoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetAlertsByCategoryResult>> Handle(GetAlertsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAlertsByCategoryAsync(request.TenantId, request.SiteId, request.CategoryCode, cancellationToken);

            return Result<GetAlertsByCategoryResult>.Success(new GetAlertsByCategoryResult(
                request.TenantId, request.CategoryCode, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Alerts for category {CategoryCode} and Tenant {TenantId}", request.CategoryCode, request.TenantId);

            return Result<GetAlertsByCategoryResult>.Failure(new ApplicationError(
                "Alerts.Alert.ByCategoryRetrievalFailed", "Unable to retrieve alerts for the given category at this time.", Guid.NewGuid()));
        }
    }
}
