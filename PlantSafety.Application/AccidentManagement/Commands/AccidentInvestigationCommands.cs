// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Commands for the real Accident investigation workflow: investigator
//   assignment, root-cause recording, CAPA corrective-action creation/completion, regulatory
//   filing, and investigation closure. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- All 6 mutating commands now implement IConcurrencyAwareCommand (Phase 4
//   AccidentManagement concurrency remediation): callers should supply the ETag captured from a
//   prior read; a stale value results in ConcurrencyConflictException. Mirrors RiskManagement's
//   commands.

using MediatR;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Commands;

/// <summary>Assigns an investigator to a Reported accident.</summary>
public sealed record AssignAccidentInvestigatorCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, Guid InvestigatorId) : IRequest<Result<AccidentStatusResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Records the outcome of Root Cause Analysis (RCA/FMEA) for an accident under investigation.</summary>
public sealed record RecordAccidentRootCauseCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, string RootCauseCategory, string Notes, Guid RecordedBy) : IRequest<Result<AccidentStatusResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Creates a new CAPA corrective action against an accident investigation.</summary>
public sealed record CreateAccidentCorrectiveActionCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, string Category, string Description, Guid ResponsiblePersonId, DateTime DueAtUtc, Guid CreatedBy)
    : IRequest<Result<CreateAccidentCorrectiveActionResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Marks a tracked corrective action Completed with evidence.</summary>
public sealed record CompleteAccidentCorrectiveActionCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, Guid CorrectiveActionId, string CompletionEvidence, Guid CompletedBy) : IRequest<Result<AccidentStatusResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Records completion of required regulatory documentation (e.g. OSHA 300/300A/301).</summary>
public sealed record FileAccidentRegulatoryReportCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, string RegulatoryAgency, string ReferenceNumber, Guid FiledBy) : IRequest<Result<AccidentStatusResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Closes an accident investigation once all mandatory prerequisites are satisfied.</summary>
public sealed record CloseAccidentInvestigationCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid AccidentId, Guid ClosedBy) : IRequest<Result<AccidentStatusResult>>, IConcurrencyAwareCommand
{
    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result payload shared by Accident lifecycle-transition commands.</summary>
/// <param name="AccidentId">Identity of the affected accident.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record AccidentStatusResult(Guid AccidentId, string Status);

/// <summary>Result of a successful <see cref="CreateAccidentCorrectiveActionCommand"/> execution.</summary>
/// <param name="AccidentId">Identity of the parent accident.</param>
/// <param name="CorrectiveActionId">Identity of the newly created corrective action.</param>
/// <param name="Status">Lifecycle status of the accident after the transition.</param>
public sealed record CreateAccidentCorrectiveActionResult(Guid AccidentId, Guid CorrectiveActionId, string Status);
