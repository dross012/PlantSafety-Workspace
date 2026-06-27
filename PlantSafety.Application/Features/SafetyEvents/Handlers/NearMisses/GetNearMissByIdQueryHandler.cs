// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetNearMissByIdQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;
using PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

/// <summary>Handles <see cref="GetNearMissByIdQuery"/>. Returns null when not found.</summary>
public sealed class GetNearMissByIdQueryHandler : IQueryHandler<GetNearMissByIdQuery, NearMissDto?>
{
    private readonly INearMissRepository _repository;
    private readonly ILogger<GetNearMissByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetNearMissByIdQueryHandler(INearMissRepository repository, ILogger<GetNearMissByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<NearMissDto?> Handle(GetNearMissByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.NearMissId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("NearMiss {NearMissId} not found for Tenant {TenantId}", request.NearMissId, request.TenantId);
            return null;
        }

        return new NearMissDto
        {
            NearMissId = model.NearMissId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            Title = model.Title,
            Description = model.Description,
            OccurredAtUtc = model.OccurredAtUtc,
            PotentialSeverity = model.PotentialSeverity,
            HazardCategory = model.HazardCategory,
            Status = model.Status,
            ReportedByWorkerId = model.ReportedByWorkerId,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
