// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for GetDrillByIdQuery.
//   Returns null when not found rather than throwing — callers may render 404 at the API layer.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.DrillsAndExercises;
using PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;

/// <summary>
/// Handles <see cref="GetDrillByIdQuery"/>.
/// Returns the full <see cref="DrillDto"/> or <c>null</c> if not found.
/// </summary>
public sealed class GetDrillByIdQueryHandler : IQueryHandler<GetDrillByIdQuery, DrillDto?>
{
    private readonly IDrillRepository _repository;
    private readonly ILogger<GetDrillByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDrillByIdQueryHandler(IDrillRepository repository, ILogger<GetDrillByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DrillDto?> Handle(GetDrillByIdQuery request, CancellationToken cancellationToken)
    {
        var readModel = await _repository.GetByIdAsync(request.TenantId, request.DrillId, cancellationToken);

        if (readModel is null)
        {
            _logger.LogDebug(
                "Drill {DrillId} not found for Tenant {TenantId} CorrelationId {CorrelationId}",
                request.DrillId, request.TenantId, request.CorrelationId);
            return null;
        }

        return new DrillDto
        {
            DrillId = readModel.DrillId,
            TenantId = readModel.TenantId,
            SiteId = readModel.SiteId,
            Title = readModel.Title,
            Description = readModel.Description,
            DrillType = readModel.DrillType,
            ScheduledAtUtc = readModel.ScheduledAtUtc,
            ExecutedAtUtc = readModel.ExecutedAtUtc,
            Status = readModel.Status,
            CoordinatorId = readModel.CoordinatorId,
            OutcomeScore = readModel.OutcomeScore,
            LastModifiedAtUtc = readModel.LastModifiedAtUtc,
            ETag = readModel.ETag
        };
    }
}
