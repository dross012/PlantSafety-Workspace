// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for RegisterFabricNodeCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.FabricNodes.RegisterNode;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;

/// <summary>Handles RegisterFabricNodeCommand. Returns the new NodeId.</summary>
public sealed class RegisterFabricNodeCommandHandler : ICommandHandler<RegisterFabricNodeCommand, Guid>
{
    private readonly IFabricNodeRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RegisterFabricNodeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterFabricNodeCommandHandler(
        IFabricNodeRepository repository,
        IDateTimeProvider clock,
        ILogger<RegisterFabricNodeCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(RegisterFabricNodeCommand request, CancellationToken cancellationToken)
    {
        var nodeId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            nodeId: nodeId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            nodeName: request.NodeName,
            nodeType: request.NodeType,
            region: request.Region,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Fabric node {NodeId} ({NodeName}) registered for Tenant {TenantId} by Actor {ActorId}",
            nodeId, request.NodeName, request.TenantId, request.ActorId);

        return nodeId;
    }
}
