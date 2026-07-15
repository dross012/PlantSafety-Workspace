// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for CreateProcessCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.ProcessManagement.Commands.Processes.CreateProcess;

namespace PlantSafety.Application.Features.ProcessManagement.Handlers.Processes;

/// <summary>Handles <see cref="CreateProcessCommand"/>. Returns the new ProcessId.</summary>
public sealed class CreateProcessCommandHandler : ICommandHandler<CreateProcessCommand, Guid>
{
    private readonly IProcessRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateProcessCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateProcessCommandHandler(
        IProcessRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateProcessCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateProcessCommand request, CancellationToken cancellationToken)
    {
        var processId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            processId: processId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            processName: request.ProcessName,
            category: request.Category,
            ownerId: request.OwnerId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "ProcessDefinition {ProcessId} created Category {Category} for Tenant {TenantId} by Actor {ActorId}",
            processId, request.Category, request.TenantId, request.ActorId);

        return processId;
    }
}
