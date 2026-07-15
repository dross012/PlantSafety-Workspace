// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to register a new integration fabric node.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.EnterpriseFabric.Commands.FabricNodes.RegisterNode;

/// <summary>
/// Registers a new integration fabric node (connector/gateway/broker/adapter) in Configuring
/// status. Caller must hold <c>EnterpriseFabric.Nodes.Register</c>.
/// </summary>
public sealed record RegisterFabricNodeCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site this node is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the fabric node.</summary>
    public required string NodeName { get; init; }

    /// <summary>Node type (e.g., Connector, Gateway, Broker, Adapter).</summary>
    public required string NodeType { get; init; }

    /// <summary>Deployment region for this node.</summary>
    public required string Region { get; init; }
}
