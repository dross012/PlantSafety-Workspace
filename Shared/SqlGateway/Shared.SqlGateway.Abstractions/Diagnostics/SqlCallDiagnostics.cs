// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Abstractions.Diagnostics
//  File:           SqlCallDiagnostics.cs
//  Created:        2026-08-09
//  Description:    Phase 2.6 deliverable: SQL diagnostics. Captured for
//                  every ISqlGateway call, success or failure, and handed
//                  to ISqlDiagnosticsSink -- the concrete SqlServerGateway
//                  implementation (Shared.SqlGateway.Infrastructure) logs
//                  it through Shared.Telemetry's ILogEventWriter (Phase
//                  2.4), so every SQL call is a structured log event with
//                  the same correlation id as the request that triggered
//                  it.
// ============================================================================
namespace Shared.SqlGateway.Abstractions.Diagnostics;

/// <summary>Diagnostics captured for one ISqlGateway call.</summary>
public sealed record SqlCallDiagnostics(
    string StoredProcedureName,
    IReadOnlyList<string> ParameterNames,
    TimeSpan ExecutionTime,
    int? RowCount,
    bool Succeeded,
    int? SqlErrorCode,
    string? ErrorMessage);
