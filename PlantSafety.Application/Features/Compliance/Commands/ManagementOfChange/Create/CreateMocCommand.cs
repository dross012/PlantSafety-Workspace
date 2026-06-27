// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to initiate a Management of Change (MOC) request.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Compliance.Commands.ManagementOfChange.Create;

/// <summary>
/// Initiates a Management of Change request in Draft status.
/// Caller must hold <c>Compliance.MOC.Create</c>.
/// </summary>
public sealed record CreateMocCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site where the change is proposed.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description of the change. Max 4000 characters.</summary>
    public required string ChangeDescription { get; init; }

    /// <summary>Change type (Permanent, Temporary, Emergency).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Risk level (Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }
}
