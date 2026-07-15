// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to register a new governed AI/ML model for oversight.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.AiGovernance.Commands.RegisterModel;

/// <summary>
/// Registers a new AI/ML model into the governed model registry in PendingReview status.
/// Caller must hold <c>AiGovernance.Models.Register</c>.
/// </summary>
public sealed record RegisterGovernedModelCommand : ICommand<Guid>
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

    /// <summary>Site this model is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Model display name.</summary>
    public required string ModelName { get; init; }

    /// <summary>Model version identifier.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Assessed risk tier (e.g., Low, Medium, High, Critical).</summary>
    public required string RiskTier { get; init; }
}
