// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateDrillCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.DrillsAndExercises.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;

/// <summary>Handles <see cref="CreateDrillCommand"/>. Returns the new DrillId.</summary>
public sealed class CreateDrillCommandHandler : ICommandHandler<CreateDrillCommand, Guid>
{
    private readonly IDrillRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateDrillCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateDrillCommandHandler(
        IDrillRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateDrillCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateDrillCommand request, CancellationToken cancellationToken)
    {
        var drillId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            drillId: drillId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            drillType: request.DrillType,
            scheduledAtUtc: request.ScheduledAtUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Drill {DrillId} scheduled for Tenant {TenantId} by Actor {ActorId}",
            drillId, request.TenantId, request.ActorId);

        return drillId;
    }
}
