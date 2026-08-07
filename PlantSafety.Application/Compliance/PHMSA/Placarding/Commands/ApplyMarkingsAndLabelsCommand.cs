// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ApplyMarkingsAndLabelsCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to apply text markings and hazard labels to a HazmatContainer
//                  (Draft -> MarkedAndLabeled). Implements IConcurrencyAwareCommand: callers
//                  should supply the ETag captured from a prior read; a stale value results in
//                  ConcurrencyConflictException. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Applies text markings and hazard labels to a HazmatContainer (Draft -&gt; MarkedAndLabeled).</summary>
/// <param name="Markings">Text markings applied (e.g. "UN1090", proper shipping name, "RQ").</param>
/// <param name="Labels">Hazard label names (e.g. "Flammable Liquid", "Corrosive", "Marine Pollutant").</param>
/// <param name="OrientationArrowsRequired">Whether orientation arrows are required on this container's packaging.</param>
/// <param name="CompatibilityGroup">Explosives compatibility group letter (A-N, S). Only meaningful for Class 1 materials.</param>
public sealed record ApplyMarkingsAndLabelsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazmatContainerId,
    IReadOnlyList<string> Markings,
    IReadOnlyList<string> Labels,
    bool OrientationArrowsRequired,
    string? CompatibilityGroup,
    Guid PerformedByUserId) : ICommand<Result<HazmatContainerStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<HazmatContainerStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
