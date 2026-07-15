// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for GetCrisisEventByIdQuery.
//   Returns null when not found rather than throwing — callers may render 404 at the API layer.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.CrisisManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.CrisisManagement.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;

/// <summary>
/// Handles <see cref="GetCrisisEventByIdQuery"/>.
/// Returns the full <see cref="CrisisEventDto"/> or <c>null</c> if not found.
/// </summary>
public sealed class GetCrisisEventByIdQueryHandler : IQueryHandler<GetCrisisEventByIdQuery, CrisisEventDto?>
{
    private readonly ICrisisEventRepository _repository;
    private readonly ILogger<GetCrisisEventByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCrisisEventByIdQueryHandler(
        ICrisisEventRepository repository,
        ILogger<GetCrisisEventByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CrisisEventDto?> Handle(GetCrisisEventByIdQuery request, CancellationToken cancellationToken)
    {
        var readModel = await _repository.GetByIdAsync(request.TenantId, request.CrisisEventId, cancellationToken);

        if (readModel is null)
        {
            _logger.LogDebug(
                "CrisisEvent {CrisisEventId} not found for Tenant {TenantId} CorrelationId {CorrelationId}",
                request.CrisisEventId, request.TenantId, request.CorrelationId);
            return null;
        }

        return new CrisisEventDto
        {
            CrisisEventId = readModel.CrisisEventId,
            TenantId = readModel.TenantId,
            SiteId = readModel.SiteId,
            Title = readModel.Title,
            Description = readModel.Description,
            CrisisCategory = readModel.CrisisCategory,
            DeclaredAtUtc = readModel.DeclaredAtUtc,
            Status = readModel.Status,
            IncidentCommanderId = readModel.IncidentCommanderId,
            ResolvedAtUtc = readModel.ResolvedAtUtc,
            LastModifiedAtUtc = readModel.LastModifiedAtUtc,
            ETag = readModel.ETag
        };
    }
}
