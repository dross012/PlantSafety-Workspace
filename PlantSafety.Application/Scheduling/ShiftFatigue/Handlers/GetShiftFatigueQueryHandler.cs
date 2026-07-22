// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetShiftFatigueQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.ShiftFatigue.Queries;
using PlantSafety.Application.Scheduling.ShiftFatigue.Repositories;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Handlers;

/// <summary>Handles <see cref="GetShiftFatigueQuery"/>.</summary>
public sealed class GetShiftFatigueQueryHandler : IRequestHandler<GetShiftFatigueQuery, Result<ShiftFatigueQueryResult>>
{
    private readonly IShiftFatigueRepository _repository;
    private readonly ILogger<GetShiftFatigueQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetShiftFatigueQueryHandler(IShiftFatigueRepository repository, ILogger<GetShiftFatigueQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ShiftFatigueQueryResult>> Handle(GetShiftFatigueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetShiftFatigueAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ShiftFatigueQueryResult>.Success(new ShiftFatigueQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ShiftFatigue for Tenant {TenantId}", request.TenantId);

            return Result<ShiftFatigueQueryResult>.Failure(new ApplicationError(
                "Scheduling.ShiftFatigue.RetrievalFailed",
                "Unable to retrieve ShiftFatigue data at this time.",
                Guid.NewGuid()));
        }
    }
}