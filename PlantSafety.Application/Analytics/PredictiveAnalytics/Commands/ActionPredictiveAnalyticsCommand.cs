// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           ActionPredictiveAnalyticsCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to record that a reviewed prediction has been acted upon (Active ->
//                  Completed, i.e. "Actioned" per PredictiveAnalyticsItemDto.Status's documented
//                  vocabulary). Implements IConcurrencyAwareCommand from the start. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CompletePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Records that a reviewed prediction has been acted upon (Active -&gt; Completed, i.e. "Actioned").</summary>
/// <param name="ActionNotes">Optional free-text notes describing the action taken.</param>
public sealed record ActionPredictiveAnalyticsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PredictiveAnalyticsId,
    Guid ActionedByUserId,
    string? ActionNotes) : ICommand<Result<PredictiveAnalyticsStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ActionedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PredictiveAnalyticsStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
