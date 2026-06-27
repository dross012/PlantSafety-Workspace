// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateNearMissCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

/// <summary>
/// Handles <see cref="CreateNearMissCommand"/>.
/// Creates a new Near Miss record and stages a <c>NearMissReportedEvent</c> in the outbox.
/// </summary>
public sealed class CreateNearMissCommandHandler : ICommandHandler<CreateNearMissCommand, Guid>
{
    private readonly INearMissRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateNearMissCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateNearMissCommandHandler(
        INearMissRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateNearMissCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateNearMissCommand request, CancellationToken cancellationToken)
    {
        var nearMissId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            nearMissId: nearMissId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            occurredAtUtc: request.OccurredAtUtc,
            potentialSeverity: request.PotentialSeverity,
            hazardCategory: request.HazardCategory,
            reportedByWorkerId: request.ReportedByWorkerId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "NearMiss {NearMissId} created for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            nearMissId, request.TenantId, request.ActorId, request.CorrelationId);

        return nearMissId;
    }
}
