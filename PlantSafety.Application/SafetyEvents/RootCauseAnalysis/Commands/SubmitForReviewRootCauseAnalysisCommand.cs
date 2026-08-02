// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           SubmitForReviewRootCauseAnalysisCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a RootCauseAnalysis for review (InProgress -> PendingReview).
//                  Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Submits a RootCauseAnalysis for review (InProgress -&gt; PendingReview).</summary>
public sealed record SubmitForReviewRootCauseAnalysisCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RootCauseAnalysisId,
    Guid SubmittedByUserId) : ICommand<Result<RootCauseAnalysisStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => SubmittedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RootCauseAnalysisStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
