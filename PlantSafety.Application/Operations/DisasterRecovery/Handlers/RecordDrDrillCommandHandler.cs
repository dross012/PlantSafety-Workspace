// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for RecordDrDrillCommand (Phase 7 §4.4 Disaster Recovery). Logs the
//   drill (date, scope, outcome, issues found) using the same ILogger-based audit convention
//   already established for feature-flag toggles and secret rotations (see
//   RecordSecretRotationCommandHandler and docs\architecture\audit-logging.md's documented gap:
//   dbo.AuditLog has no INSERT-capable path from C# for non-tabular events with no backing SQL
//   row of their own).
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.DisasterRecovery.Commands;
using PlantSafety.SharedKernel.Audit;

namespace PlantSafety.Application.Operations.DisasterRecovery.Handlers;

/// <summary>Handles <see cref="RecordDrDrillCommand"/>.</summary>
public sealed class RecordDrDrillCommandHandler : IRequestHandler<RecordDrDrillCommand, Result<Guid>>
{
    private readonly ILogger<RecordDrDrillCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordDrDrillCommandHandler(ILogger<RecordDrDrillCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<Guid>> Handle(RecordDrDrillCommand request, CancellationToken cancellationToken)
    {
        var drillId = Guid.NewGuid();
        var auditEvent = new DrDrillResult(drillId, request.DrillDateUtc, request.Scope, request.Outcome, request.IssuesFound);

        var logLevel = string.Equals(request.Outcome, "Passed", StringComparison.OrdinalIgnoreCase)
            ? LogLevel.Information
            : LogLevel.Warning;

        _logger.Log(
            logLevel,
            "DR drill recorded: DrillId={DrillId}; DrillDateUtc={DrillDateUtc:O}; Scope={Scope}; Outcome={Outcome}; " +
            "IssuesFoundCount={IssuesFoundCount}; IssuesFound={IssuesFound}; EventType={EventType}",
            auditEvent.DrillId, auditEvent.DrillDateUtc, auditEvent.Scope, auditEvent.Outcome,
            auditEvent.IssuesFound.Count, string.Join("; ", auditEvent.IssuesFound), auditEvent.EventType);

        return Task.FromResult(Result<Guid>.Success(drillId));
    }
}
