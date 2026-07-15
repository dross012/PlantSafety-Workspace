// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for GetEmergencyResponseByIdQuery.
//   Returns null when not found rather than throwing — callers may render 404 at the API layer.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.EmergencyManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.EmergencyManagement.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

/// <summary>
/// Handles <see cref="GetEmergencyResponseByIdQuery"/>.
/// Returns the full <see cref="EmergencyResponseDto"/> or <c>null</c> if not found.
/// </summary>
public sealed class GetEmergencyResponseByIdQueryHandler : IQueryHandler<GetEmergencyResponseByIdQuery, EmergencyResponseDto?>
{
    private readonly IEmergencyResponseRepository _repository;
    private readonly ILogger<GetEmergencyResponseByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyResponseByIdQueryHandler(
        IEmergencyResponseRepository repository,
        ILogger<GetEmergencyResponseByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<EmergencyResponseDto?> Handle(GetEmergencyResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var readModel = await _repository.GetByIdAsync(request.TenantId, request.EmergencyResponseId, cancellationToken);

        if (readModel is null)
        {
            _logger.LogDebug(
                "EmergencyResponse {EmergencyResponseId} not found for Tenant {TenantId} CorrelationId {CorrelationId}",
                request.EmergencyResponseId, request.TenantId, request.CorrelationId);
            return null;
        }

        return new EmergencyResponseDto
        {
            EmergencyResponseId = readModel.ResponseId,
            TenantId = readModel.TenantId,
            SiteId = readModel.SiteId,
            LinkedCrisisEventId = readModel.LinkedCrisisEventId,
            ResponsePlanName = readModel.ResponsePlanName,
            ActivatedAtUtc = readModel.ActivatedAtUtc,
            Status = readModel.Status,
            CoordinatorId = readModel.CoordinatorId,
            StandDownAtUtc = readModel.StandDownAtUtc,
            LastModifiedAtUtc = readModel.LastModifiedAtUtc,
            ETag = readModel.ETag
        };
    }
}
