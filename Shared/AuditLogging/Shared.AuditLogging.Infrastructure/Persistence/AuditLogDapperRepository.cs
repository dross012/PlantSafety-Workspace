// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           AuditLogDapperRepository.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed, READ-ONLY implementation of IAuditLogRepository over the
//                  shared [PlantSafety.AuditDB].dbo.AuditLog table, calling the
//                  dbo.usp_SearchAuditLog stored procedure (see ..\..\sql\
//                  Phase1-StoredProcedures\01-usp-search-auditlog.sql). This table is
//                  server-level, cross-database platform state written to by hundreds of
//                  stored procedures via the dbo.AuditLog synonym in each of the 8 module
//                  databases -- not tenant-scoped RLS data -- so this repository opens its
//                  own SqlConnection from AuditLoggingOptions.AuditDbConnectionString
//                  rather than going through a tenant-scoped connection factory, matching
//                  the pattern PlantSafety's original AuditLogDapperRepository established
//                  (PlantSafety.Infrastructure's Operations.Backup/SchemaGuard modules).
//
//                  SAFETY: This repository issues a single stored-procedure call
//                  (dbo.usp_SearchAuditLog) that itself only SELECTs. It must never gain
//                  an INSERT/UPDATE/DELETE method -- AuditLog is append-only audit
//                  infrastructure written exclusively by the canonical stored-procedure
//                  template's best-effort audit block.
//
//                  Extracted from PlantSafety.Infrastructure's Operations.AuditViewer.
//                  Persistence.AuditLogDapperRepository (created 2026-07-20) on
//                  2026-07-26. The original inline SELECT/COUNT SQL text was converted
//                  into a parameterized stored procedure (dbo.usp_SearchAuditLog) as part
//                  of this extraction, per the house convention of Dapper + stored
//                  procedures rather than ad hoc inline SQL for cross-module contracts.
//                  Additive-only evolution.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.Exceptions;
using Shared.AuditLogging.Infrastructure.Options;

namespace Shared.AuditLogging.Infrastructure.Persistence;

/// <summary>Dapper-backed, read-only repository over the shared dbo.AuditLog table.</summary>
public sealed class AuditLogDapperRepository : IAuditLogRepository
{
    private const string ProcedureName = "dbo.usp_SearchAuditLog";

    private readonly AuditLoggingOptions _options;
    private readonly ILogger<AuditLogDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AuditLogDapperRepository(IOptions<AuditLoggingOptions> options, ILogger<AuditLogDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private SqlConnection CreateAuditDbConnection()
    {
        var connection = new SqlConnection(_options.AuditDbConnectionString);
        connection.Open();
        return connection;
    }

    /// <inheritdoc/>
    public async Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var pageNumber = criteria.PageNumber < 1 ? 1 : criteria.PageNumber;
        var pageSize = criteria.PageSize < 1 ? 50 : Math.Min(criteria.PageSize, _options.MaxPageSize);

        var parameters = new DynamicParameters();
        parameters.Add("@TableName", criteria.TableName);
        parameters.Add("@Operation", criteria.Operation);
        parameters.Add("@ChangedBy", criteria.ChangedBy);
        parameters.Add("@Module", criteria.Module);
        parameters.Add("@Severity", criteria.Severity);
        parameters.Add("@FromUtc", criteria.FromUtc);
        parameters.Add("@ToUtc", criteria.ToUtc);
        parameters.Add("@SearchText", criteria.SearchText);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);

        try
        {
            using var connection = CreateAuditDbConnection();

            // dbo.usp_SearchAuditLog returns two result sets: (1) a single-row total count,
            // (2) the requested page of rows, newest first -- see the stored procedure's
            // header comment for why two result sets were chosen over an OUTPUT parameter.
            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    ProcedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: _options.SearchCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var totalCount = await multi.ReadFirstAsync<int>();
            var rows = await multi.ReadAsync<AuditLogRow>();

            var items = rows.Select(r => new AuditLogEntry(
                r.TableName,
                r.SourceDatabase,
                r.Operation,
                r.ChangedBy,
                r.ChangeTime,
                r.ViewerAccessed,
                r.ErrorContext,
                r.Severity,
                r.Module,
                r.BeforeSnapshot,
                r.AfterSnapshot)).ToList();

            return new AuditLogSearchResult(items, pageNumber, pageSize, totalCount);
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber} while searching " +
                "PlantSafety.AuditDB.dbo.AuditLog.",
                ProcedureName, ex.Number);

            throw new AuditLogQueryException(
                $"Failed to search PlantSafety.AuditDB.dbo.AuditLog via {ProcedureName}.", ex);
        }
    }

    /// <summary>Raw projection of dbo.AuditLog before mapping to <see cref="AuditLogEntry"/>.</summary>
    private sealed class AuditLogRow
    {
        public string TableName { get; init; } = string.Empty;
        public string SourceDatabase { get; init; } = string.Empty;
        public string Operation { get; init; } = string.Empty;
        public string ChangedBy { get; init; } = string.Empty;
        public DateTime ChangeTime { get; init; }
        public bool ViewerAccessed { get; init; }
        public string? ErrorContext { get; init; }
        public int Severity { get; init; }
        public string Module { get; init; } = string.Empty;
        public string? BeforeSnapshot { get; init; }
        public string? AfterSnapshot { get; init; }
    }
}
