// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetWebinarsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Training.Webinars.Queries;
using PlantSafety.Application.Operational.Training.Webinars.Repositories;

namespace PlantSafety.Application.Operational.Training.Webinars.Handlers;

/// <summary>Handles <see cref="GetWebinarsQuery"/>.</summary>
public sealed class GetWebinarsQueryHandler : IRequestHandler<GetWebinarsQuery, Result<WebinarsQueryResult>>
{
    private readonly IWebinarsRepository _repository;
    private readonly ILogger<GetWebinarsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWebinarsQueryHandler(IWebinarsRepository repository, ILogger<GetWebinarsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WebinarsQueryResult>> Handle(GetWebinarsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWebinarsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WebinarsQueryResult>.Success(new WebinarsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Webinars for Tenant {TenantId}", request.TenantId);

            return Result<WebinarsQueryResult>.Failure(new ApplicationError(
                "Operational.Training.Webinars.RetrievalFailed",
                "Unable to retrieve webinar data at this time.",
                Guid.NewGuid()));
        }
    }
}
