// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.AuditViewer
//  Layer:          Infrastructure
//  File:           AuditLogDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed, READ-ONLY implementation of IAuditLogRepository over the
//                  shared [PlantSafety.AuditDB].dbo.AuditLog table (Phase8-Structural\
//                  83-synonyms-audit-error.sql). This table is server-level, cross-database
//                  platform state written to by hundreds of stored procedures via the
//                  dbo.AuditLog synonym in each of the 8 module databases -- not
//                  tenant-scoped RLS data -- so this repository opens its own SqlConnection
//                  from AuditViewerOptions.AuditDbConnectionString rather than going through
//                  ISqlConnectionFactory/DapperQueryExecutor, matching the
//                  Operations.Backup/SchemaGuard pattern. SqlException handling routes
//                  through the shared ErrorClassifier.
//
//                  SAFETY: This repository issues SELECT statements only. It must never
//                  gain an INSERT/UPDATE/DELETE method -- AuditLog is append-only audit
//                  infrastructure written exclusively by the canonical stored-procedure
//                  template's best-effort audit block.
//
//                  SQL INJECTION DISCIPLINE: The WHERE clause is built by conditionally
//                  appending parameterized predicate fragments (structural presence/absence
//                  only). Every filter VALUE is always bound via Dapper's DynamicParameters
//                  -- no filter value is ever concatenated into the SQL text.
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.AuditViewer.Models;
using PlantSafety.Application.Operations.AuditViewer.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.AuditViewer.Options;

namespace PlantSafety.Infrastructure.Operations.AuditViewer.Persistence;

/// <summary>Dapper-backed, read-only repository over the shared dbo.AuditLog table.</summary>
public sealed class AuditLogDapperRepository : IAuditLogRepository
{
    private readonly AuditViewerOptions _options;
    private readonly ILogger<AuditLogDapperRepository> _logger;

    private const string SelectColumns = @"
    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
    ViewerAccessed, ErrorContext, Severity, Module, BeforeSnapshot, AfterSnapshot";

    /// <summary>Initialises dependencies.</summary>
    public AuditLogDapperRepository(IOptions<AuditViewerOptions> options, ILogger<AuditLogDapperRepository> logger)
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
    public async Task<AuditLogSearchResultDto> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var pageNumber = criteria.PageNumber < 1 ? 1 : criteria.PageNumber;
        var pageSize = criteria.PageSize < 1 ? 50 : Math.Min(criteria.PageSize, _options.MaxPageSize);

        var (whereClause, parameters) = BuildWhereClause(criteria);

        var countSql = $"SELECT COUNT(*) FROM dbo.AuditLog{whereClause};";
        var pageSql = $@"
SELECT{SelectColumns}
FROM dbo.AuditLog{whereClause}
ORDER BY ChangeTime DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        parameters.Add("@Offset", (pageNumber - 1) * pageSize);
        parameters.Add("@PageSize", pageSize);

        try
        {
            using var connection = CreateAuditDbConnection();

            var totalCount = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    countSql,
                    parameters,
                    commandTimeout: _options.SearchCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var rows = await connection.QueryAsync<AuditLogRow>(
                new CommandDefinition(
                    pageSql,
                    parameters,
                    commandTimeout: _options.SearchCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var items = rows.Select(r => new AuditLogEntryDto(
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

            return new AuditLogSearchResultDto(items, pageNumber, pageSize, totalCount);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "PlantSafety.AuditDB.dbo.AuditLog", _logger);
        }
    }

    /// <summary>
    /// Builds a parameterized WHERE clause. Only the STRUCTURAL presence/absence of a
    /// predicate is conditional on which filters were supplied -- every filter VALUE is
    /// always bound as a Dapper parameter, never concatenated into the SQL text.
    /// </summary>
    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(AuditLogSearchCriteria criteria)
    {
        var predicates = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(criteria.TableName))
        {
            predicates.Add("TableName = @TableName");
            parameters.Add("@TableName", criteria.TableName);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Operation))
        {
            predicates.Add("Operation = @Operation");
            parameters.Add("@Operation", criteria.Operation);
        }

        if (!string.IsNullOrWhiteSpace(criteria.ChangedBy))
        {
            predicates.Add("ChangedBy = @ChangedBy");
            parameters.Add("@ChangedBy", criteria.ChangedBy);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Module))
        {
            predicates.Add("Module = @Module");
            parameters.Add("@Module", criteria.Module);
        }

        if (criteria.Severity.HasValue)
        {
            predicates.Add("Severity = @Severity");
            parameters.Add("@Severity", criteria.Severity.Value);
        }

        if (criteria.FromUtc.HasValue)
        {
            predicates.Add("ChangeTime >= @FromUtc");
            parameters.Add("@FromUtc", criteria.FromUtc.Value);
        }

        if (criteria.ToUtc.HasValue)
        {
            predicates.Add("ChangeTime <= @ToUtc");
            parameters.Add("@ToUtc", criteria.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            predicates.Add("ErrorContext LIKE @SearchText");
            parameters.Add("@SearchText", $"%{criteria.SearchText}%");
        }

        if (predicates.Count == 0)
            return (string.Empty, parameters);

        var sb = new StringBuilder(" WHERE ");
        sb.Append(string.Join(" AND ", predicates));
        return (sb.ToString(), parameters);
    }

    /// <summary>Raw projection of dbo.AuditLog before DTO mapping.</summary>
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
