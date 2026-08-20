// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Handler for GetWellnessQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Wellness.Queries;
using PlantSafety.Application.Operational.Wellness.Repositories;

namespace PlantSafety.Application.Operational.Wellness.Handlers;

/// <summary>Handles <see cref="GetWellnessQuery"/>.</summary>
public sealed class GetWellnessQueryHandler : IRequestHandler<GetWellnessQuery, Result<WellnessQueryResult>>
{
    private readonly IWellnessRepository _repository;
    private readonly ILogger<GetWellnessQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWellnessQueryHandler(IWellnessRepository repository, ILogger<GetWellnessQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WellnessQueryResult>> Handle(GetWellnessQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWellnessAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WellnessQueryResult>.Success(new WellnessQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Wellness for Tenant {TenantId}", request.TenantId);

            return Result<WellnessQueryResult>.Failure(new ApplicationError(
                "Operational.Wellness.RetrievalFailed",
                "Unable to retrieve Wellness data at this time.",
                Guid.NewGuid()));
        }
    }
}