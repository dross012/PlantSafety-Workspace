// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListAccessEventsByZoneQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories;

namespace PlantSafety.Application.PhysicalAccessControl.AccessEvents.Handlers;

/// <summary>Handles <see cref="ListAccessEventsByZoneQuery"/>.</summary>
public sealed class ListAccessEventsByZoneQueryHandler : IRequestHandler<ListAccessEventsByZoneQuery, Result<ListAccessEventsByZoneResult>>
{
    private readonly IAccessEventReadRepository _repository;
    private readonly ILogger<ListAccessEventsByZoneQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListAccessEventsByZoneQueryHandler(IAccessEventReadRepository repository, ILogger<ListAccessEventsByZoneQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListAccessEventsByZoneResult>> Handle(ListAccessEventsByZoneQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListByZoneAsync(request.TenantId, request.SafetyZoneId, request.From, request.To, cancellationToken);

            return Result<ListAccessEventsByZoneResult>.Success(new ListAccessEventsByZoneResult(request.SafetyZoneId, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list AccessEvents for SafetyZone {SafetyZoneId}", request.SafetyZoneId);

            return Result<ListAccessEventsByZoneResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessEvent.RetrievalFailed",
                "Unable to retrieve AccessEvent records at this time.",
                Guid.NewGuid()));
        }
    }
}
