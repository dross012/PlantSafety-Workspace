// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreatePermitCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Create;

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Handles <see cref="CreatePermitCommand"/>. Returns the new PermitId.</summary>
public sealed class CreatePermitCommandHandler : ICommandHandler<CreatePermitCommand, Guid>
{
    private readonly IPermitRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreatePermitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreatePermitCommandHandler(
        IPermitRepository repository,
        IDateTimeProvider clock,
        ILogger<CreatePermitCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreatePermitCommand request, CancellationToken cancellationToken)
    {
        var permitId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            permitId: permitId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            permitType: request.PermitType,
            title: request.Title,
            description: request.Description,
            workLeadId: request.WorkLeadId,
            validFromUtc: request.ValidFromUtc,
            validToUtc: request.ValidToUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Permit {PermitId} created for Tenant {TenantId} Type {PermitType} by Actor {ActorId}",
            permitId, request.TenantId, request.PermitType, request.ActorId);

        return permitId;
    }
}
