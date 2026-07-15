// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for RegisterDigitalTwinCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.DigitalTwin.Commands.RegisterTwin;

namespace PlantSafety.Application.Features.DigitalTwin.Handlers;

/// <summary>Handles <see cref="RegisterDigitalTwinCommand"/>. Returns the new TwinId.</summary>
public sealed class RegisterDigitalTwinCommandHandler : ICommandHandler<RegisterDigitalTwinCommand, Guid>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RegisterDigitalTwinCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterDigitalTwinCommandHandler(
        IDigitalTwinRepository repository,
        IDateTimeProvider clock,
        ILogger<RegisterDigitalTwinCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(RegisterDigitalTwinCommand request, CancellationToken cancellationToken)
    {
        var twinId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            twinId: twinId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            assetName: request.AssetName,
            assetType: request.AssetType,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "DigitalTwin {TwinId} registered for asset {AssetName} Tenant {TenantId} by Actor {ActorId}",
            twinId, request.AssetName, request.TenantId, request.ActorId);

        return twinId;
    }
}
