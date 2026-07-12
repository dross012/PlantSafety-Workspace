// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetExcavationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Excavation.Queries;
using PlantSafety.Application.Compliance.Permits.Excavation.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Excavation.Handlers;

/// <summary>Handles <see cref="GetExcavationQuery"/>.</summary>
public sealed class GetExcavationQueryHandler : IRequestHandler<GetExcavationQuery, Result<ExcavationQueryResult>>
{
    private readonly IExcavationRepository _repository;
    private readonly ILogger<GetExcavationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetExcavationQueryHandler(IExcavationRepository repository, ILogger<GetExcavationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExcavationQueryResult>> Handle(GetExcavationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetExcavationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ExcavationQueryResult>.Success(new ExcavationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Excavation for Tenant {TenantId}", request.TenantId);

            return Result<ExcavationQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.Excavation.RetrievalFailed",
                "Unable to retrieve excavation permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
