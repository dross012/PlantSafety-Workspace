// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateMocCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Compliance.Commands.ManagementOfChange.Create;

namespace PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;

/// <summary>Handles <see cref="CreateMocCommand"/>. Returns the new MocId.</summary>
public sealed class CreateMocCommandHandler : ICommandHandler<CreateMocCommand, Guid>
{
    private readonly IMocRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateMocCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateMocCommandHandler(
        IMocRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateMocCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateMocCommand request, CancellationToken cancellationToken)
    {
        var mocId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            mocId: mocId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            changeType: request.ChangeType,
            riskLevel: request.RiskLevel,
            initiatorId: request.InitiatorId,
            proposedStartUtc: request.ProposedStartUtc,
            proposedEndUtc: request.ProposedEndUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "MOC {MocId} created for Tenant {TenantId} ChangeType {ChangeType} by Actor {ActorId}",
            mocId, request.TenantId, request.ChangeType, request.ActorId);

        return mocId;
    }
}
