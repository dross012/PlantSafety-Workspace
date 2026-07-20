// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog,
//   newest first, with optional severity/date filters and paging.
// SchemaVersion: 1

using System;
using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Queries;

/// <summary>Retrieves recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog, newest first.</summary>
/// <param name="MinSeverity">Optional minimum ErrorSeverity (inclusive). Null returns all severities.</param>
/// <param name="SinceUtc">Optional lower bound on ErrorDate (inclusive). Null returns all history.</param>
/// <param name="Top">Page size. Defaults to 50.</param>
/// <param name="Skip">Rows to skip for pagination. Defaults to 0.</param>
/// <param name="MaxSeverity">Optional maximum ErrorSeverity (inclusive), paired with <paramref name="MinSeverity"/> to filter to a severity band. Null returns all severities up to no cap.</param>
/// <param name="SourceDatabase">Optional exact match on the SourceDatabase column (the database the failing procedure executed against). Null returns all databases.</param>
/// <param name="Module">Optional exact match on the Module column (the stored procedure/object name, OBJECT_NAME(@@PROCID)). Null returns all modules.</param>
/// <param name="ErrorProcedure">Optional exact match on the ErrorProcedure column. Null returns all procedures.</param>
public sealed record GetErrorLogQuery(
    int? MinSeverity,
    DateTime? SinceUtc,
    int Top = 50,
    int Skip = 0,
    int? MaxSeverity = null,
    string? SourceDatabase = null,
    string? Module = null,
    string? ErrorProcedure = null)
    : IRequest<Result<IReadOnlyList<ErrorLogItemDto>>>;
