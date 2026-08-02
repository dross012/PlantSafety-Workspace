// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           ArchiveRegulatoryCalendarCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to archive a closed RegulatoryCalendar obligation for compliance
//                  retention (Closed -> Archived). Implements IConcurrencyAwareCommand from the
//                  start. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ArchivePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Archives a closed RegulatoryCalendar obligation for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveRegulatoryCalendarCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RegulatoryCalendarId,
    Guid PerformedByUserId) : ICommand<Result<RegulatoryCalendarStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RegulatoryCalendarStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
