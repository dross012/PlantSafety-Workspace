// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Abstractions.Diagnostics
//  File:           SqlDiagnosticsLogEventFactory.cs
//  Created:        2026-08-09
//  Description:    Builds a Shared.Telemetry ILogEvent from SqlCallDiagnostics,
//                  so SqlServerGateway (Infrastructure) can log every call
//                  through the same ILogEventWriter/LogCategory machinery
//                  as everything else (Phase 2.4), rather than a separate,
//                  parallel SQL-specific logging path.
// ============================================================================
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SqlGateway.Abstractions.Diagnostics;

/// <summary>Builds a structured ILogEvent from a SqlCallDiagnostics result.</summary>
public static class SqlDiagnosticsLogEventFactory
{
    /// <summary>Builds the log event for the given diagnostics.</summary>
    public static LogEvent ToLogEvent(SqlCallDiagnostics diagnostics, string correlationId, DateTime occurredAtUtc) => new(
        EventId: diagnostics.Succeeded ? "SQL_CALL_SUCCEEDED" : "SQL_CALL_FAILED",
        Category: LogCategory.Infrastructure,
        Severity: diagnostics.Succeeded ? LogSeverity.Info : LogSeverity.Error,
        Message: diagnostics.Succeeded
            ? $"{diagnostics.StoredProcedureName} completed in {diagnostics.ExecutionTime.TotalMilliseconds:F1}ms."
            : $"{diagnostics.StoredProcedureName} failed after {diagnostics.ExecutionTime.TotalMilliseconds:F1}ms: {diagnostics.ErrorMessage}",
        Payload: new Dictionary<string, object?>
        {
            ["StoredProcedureName"] = diagnostics.StoredProcedureName,
            ["ParameterNames"] = string.Join(",", diagnostics.ParameterNames),
            ["ExecutionTimeMs"] = diagnostics.ExecutionTime.TotalMilliseconds,
            ["RowCount"] = diagnostics.RowCount,
            ["SqlErrorCode"] = diagnostics.SqlErrorCode,
        },
        CorrelationId: correlationId,
        Module: "SqlGateway",
        OccurredAtUtc: occurredAtUtc);
}
