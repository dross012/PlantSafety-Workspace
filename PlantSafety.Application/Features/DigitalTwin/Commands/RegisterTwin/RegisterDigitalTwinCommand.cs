// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to register a new Digital Twin for a physical plant asset.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.DigitalTwin.Commands.RegisterTwin;

/// <summary>
/// Registers a new Digital Twin in Provisioning status. Caller must hold
/// <c>DigitalTwin.Twins.Register</c>.
/// </summary>
public sealed record RegisterDigitalTwinCommand : ICommand<Guid>
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

    /// <summary>Site this twin is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the physical asset this twin represents.</summary>
    public required string AssetName { get; init; }

    /// <summary>Asset type/category (e.g., Machine, Line, Facility).</summary>
    public required string AssetType { get; init; }
}
