// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListExpiringOperatorQualificationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;

/// <summary>Handles <see cref="ListExpiringOperatorQualificationsQuery"/>.</summary>
public sealed class ListExpiringOperatorQualificationsQueryHandler : IRequestHandler<ListExpiringOperatorQualificationsQuery, Result<IReadOnlyList<CraneHoistSystemsItemDto>>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<ListExpiringOperatorQualificationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListExpiringOperatorQualificationsQueryHandler(ICraneHoistSystemsRepository repository, ILogger<ListExpiringOperatorQualificationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CraneHoistSystemsItemDto>>> Handle(ListExpiringOperatorQualificationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListExpiringOperatorQualificationsAsync(request.TenantId, request.SiteId, request.WithinDays, cancellationToken);
            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list expiring operator qualifications for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.ListExpiringQualificationsFailed",
                "Unable to list expiring CraneHoistSystems operator qualifications at this time.",
                Guid.NewGuid()));
        }
    }
}
