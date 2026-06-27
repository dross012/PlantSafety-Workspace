// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateEmergencyResponseCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.EmergencyManagement.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

/// <summary>Handles <see cref="CreateEmergencyResponseCommand"/>. Returns the new ResponseId.</summary>
public sealed class CreateEmergencyResponseCommandHandler : ICommandHandler<CreateEmergencyResponseCommand, Guid>
{
    private readonly IEmergencyResponseRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateEmergencyResponseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateEmergencyResponseCommandHandler(
        IEmergencyResponseRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateEmergencyResponseCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateEmergencyResponseCommand request, CancellationToken cancellationToken)
    {
        var responseId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            responseId: responseId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            linkedCrisisEventId: request.LinkedCrisisEventId,
            responsePlanName: request.ResponsePlanName,
            description: request.Description,
            activatedAtUtc: request.ActivatedAtUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "EmergencyResponse {ResponseId} activated for Tenant {TenantId} by Actor {ActorId}",
            responseId, request.TenantId, request.ActorId);

        return responseId;
    }
}
