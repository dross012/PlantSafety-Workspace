// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           ErrorLogItemDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single row from the shared, cross-database platform table
//                  [PlantSafety.ErrorDB].dbo.ErrorLog (confirmed live from the INSERT
//                  statements in Phase17-Reliability\170-fix-errorlog-masking-bug.sql —
//                  columns: ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure,
//                  ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters,
//                  SourceDatabase, Module — written by every canonical Phase6 stored
//                  procedure's CATCH block). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Monitoring.Models;

/// <summary>A single row from [PlantSafety.ErrorDB].dbo.ErrorLog.</summary>
/// <param name="ErrorNumber">SQL Server ERROR_NUMBER() at the time of the failure.</param>
/// <param name="ErrorSeverity">SQL Server ERROR_SEVERITY().</param>
/// <param name="ErrorState">SQL Server ERROR_STATE().</param>
/// <param name="ErrorProcedure">The stored procedure that raised the error, if any.</param>
/// <param name="ErrorLine">The line within the procedure that raised the error.</param>
/// <param name="ErrorMessage">SQL Server ERROR_MESSAGE().</param>
/// <param name="ErrorDateUtc">When the error was logged.</param>
/// <param name="ErrorContext">Caller-supplied parameter context (e.g. "AiGovernanceId=...; CreatedBy=...").</param>
/// <param name="ErrorParameters">Structured detail string (ErrorNumber/State/Severity/Procedure/Line/Message concatenation).</param>
/// <param name="SourceDatabase">The database the failing procedure executed against.</param>
/// <param name="Module">The stored procedure/object name (OBJECT_NAME(@@PROCID)).</param>
public sealed record ErrorLogItemDto(
    int? ErrorNumber,
    int? ErrorSeverity,
    int? ErrorState,
    string? ErrorProcedure,
    int? ErrorLine,
    string? ErrorMessage,
    DateTime ErrorDateUtc,
    string? ErrorContext,
    string? ErrorParameters,
    string? SourceDatabase,
    string? Module);
