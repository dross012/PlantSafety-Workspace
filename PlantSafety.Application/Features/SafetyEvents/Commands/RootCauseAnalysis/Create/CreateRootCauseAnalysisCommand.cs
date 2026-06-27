// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to initiate a Root Cause Analysis for a linked safety event.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.RootCauseAnalysis.Create;

/// <summary>
/// Initiates a Root Cause Analysis linked to an Incident or NearMiss.
/// Caller must hold <c>SafetyEvents.RCA.Create</c>.
/// </summary>
public sealed record CreateRootCauseAnalysisCommand : ICommand<Guid>
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

    /// <summary>The Incident or NearMiss this RCA is linked to.</summary>
    public required Guid LinkedEventId { get; init; }

    /// <summary>Type of linked event: Incident, NearMiss.</summary>
    public required string LinkedEventType { get; init; }

    /// <summary>RCA methodology (e.g., 5Why, FishboneDiagram, FaultTree).</summary>
    public required string Methodology { get; init; }

    /// <summary>Worker leading this RCA.</summary>
    public required Guid LeadInvestigatorId { get; init; }
}
