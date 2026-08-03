// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           SafetyDataDapperRepository.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed, READ-ONLY implementation of ISafetyDataRepository
//                  over [PlantSafety.CoreDB]'s [RiskMgmt].[Risk] /
//                  [RiskMgmt].[ControlMeasure] / [ContinuousImprovement].[CAPA]
//                  tables, calling dbo.usp_GetHazardSummary,
//                  dbo.usp_GetCorrectiveActions, and dbo.usp_GetSafetyCompliance
//                  (see ..\..\sql\Phase1-StoredProcedures). Mirrors
//                  Shared.AuditLogging.Infrastructure.Persistence.
//                  AuditLogDapperRepository's shape (own SqlConnection from
//                  options, DynamicParameters, QueryMultipleAsync for
//                  count-then-page, SqlException -> module-specific exception).
//
//                  SAFETY: Each method issues a single stored-procedure call that
//                  itself only SELECTs. This class must never gain an
//                  INSERT/UPDATE/DELETE method -- PlantSafety's own aggregates
//                  own all writes to these tables.
//                  Additive-only evolution.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.SafetyData.Abstractions.Models;
using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.Exceptions;
using Shared.SafetyData.Infrastructure.Options;

namespace Shared.SafetyData.Infrastructure.Persistence;

/// <summary>Dapper-backed, read-only repository over PlantSafety's Hazard/Risk, Corrective Action, and Safety Compliance data.</summary>
public sealed class SafetyDataDapperRepository : ISafetyDataRepository
{
    private const string HazardSummaryProcedureName = "dbo.usp_GetHazardSummary";
    private const string CorrectiveActionsProcedureName = "dbo.usp_GetCorrectiveActions";
    private const string SafetyComplianceProcedureName = "dbo.usp_GetSafetyCompliance";

    private readonly SafetyDataOptions _options;
    private readonly ILogger<SafetyDataDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SafetyDataDapperRepository(IOptions<SafetyDataOptions> options, ILogger<SafetyDataDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private SqlConnection CreateCoreDbConnection()
    {
        var connection = new SqlConnection(_options.CoreDbConnectionString);
        connection.Open();
        return connection;
    }

    /// <inheritdoc/>
    public async Task<SafetySearchResult<HazardSummaryEntry>> GetHazardSummaryAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var (pageNumber, pageSize) = NormalizePaging(criteria);

        var parameters = new DynamicParameters();
        parameters.Add("@TenantId", criteria.TenantId);
        parameters.Add("@SiteId", criteria.SiteId);
        parameters.Add("@FromUtc", criteria.FromUtc);
        parameters.Add("@ToUtc", criteria.ToUtc);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);

        try
        {
            using var connection = CreateCoreDbConnection();

            // dbo.usp_GetHazardSummary returns three result sets: (1) a single-row total
            // count, (2) the requested page of RiskMgmt.Risk rows, newest first, (3) the
            // RiskMgmt.ControlMeasure rows for every RiskId in result set 2 -- see the
            // stored procedure's header comment for why control measures are returned as
            // a separate result set rather than a joined/duplicated row set.
            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    HazardSummaryProcedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: _options.QueryCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var totalCount = await multi.ReadFirstAsync<int>();
            var riskRows = (await multi.ReadAsync<RiskRow>()).ToList();
            var controlMeasureRows = (await multi.ReadAsync<ControlMeasureRow>()).ToList();

            var controlMeasuresByRiskId = controlMeasureRows
                .GroupBy(cm => cm.RiskId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(cm => cm.Description).ToList());

            var items = riskRows.Select(r => new HazardSummaryEntry(
                r.RiskId.ToString(),
                r.CreatedAtUtc,
                r.HazardSource,
                r.Category,
                r.RiskTitle,
                controlMeasuresByRiskId.TryGetValue(r.RiskId, out var cms) ? cms : Array.Empty<string>())).ToList();

            return new SafetySearchResult<HazardSummaryEntry>(items, pageNumber, pageSize, totalCount);
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber} while querying " +
                "PlantSafety.CoreDB.RiskMgmt.Risk.",
                HazardSummaryProcedureName, ex.Number);

            throw new SafetyDataQueryException(
                $"Failed to retrieve hazard summary data via {HazardSummaryProcedureName}.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SafetySearchResult<CorrectiveActionEntry>> GetCorrectiveActionsAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var (pageNumber, pageSize) = NormalizePaging(criteria);

        var parameters = new DynamicParameters();
        parameters.Add("@TenantId", criteria.TenantId);
        parameters.Add("@SiteId", criteria.SiteId);
        parameters.Add("@FromUtc", criteria.FromUtc);
        parameters.Add("@ToUtc", criteria.ToUtc);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);

        try
        {
            using var connection = CreateCoreDbConnection();

            // dbo.usp_GetCorrectiveActions returns two result sets: (1) a single-row total
            // count, (2) the requested page of ContinuousImprovement.CAPA rows, newest first.
            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    CorrectiveActionsProcedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: _options.QueryCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var totalCount = await multi.ReadFirstAsync<int>();
            var rows = await multi.ReadAsync<CapaRow>();

            var items = rows.Select(r => new CorrectiveActionEntry(
                r.CapaId.ToString(),
                r.CreatedAtUtc,
                r.ActionType,
                r.Status,
                r.Title,
                r.DueDateUtc)).ToList();

            return new SafetySearchResult<CorrectiveActionEntry>(items, pageNumber, pageSize, totalCount);
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber} while querying " +
                "PlantSafety.CoreDB.ContinuousImprovement.CAPA.",
                CorrectiveActionsProcedureName, ex.Number);

            throw new SafetyDataQueryException(
                $"Failed to retrieve corrective action data via {CorrectiveActionsProcedureName}.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyComplianceEntry>> GetSafetyComplianceAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var parameters = new DynamicParameters();
        parameters.Add("@TenantId", criteria.TenantId);
        parameters.Add("@SiteId", criteria.SiteId);
        parameters.Add("@FromUtc", criteria.FromUtc);
        parameters.Add("@ToUtc", criteria.ToUtc);

        try
        {
            using var connection = CreateCoreDbConnection();

            var rows = await connection.QueryAsync<SafetyComplianceRow>(
                new CommandDefinition(
                    SafetyComplianceProcedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: _options.QueryCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            return rows.Select(r => new SafetyComplianceEntry(
                r.Category,
                r.TotalCount,
                r.OpenCount,
                r.ClosedCount,
                r.OverdueCount,
                r.CompliancePercentage,
                r.AsOfUtc)).ToList();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber} while computing " +
                "PlantSafety.CoreDB safety compliance rollup.",
                SafetyComplianceProcedureName, ex.Number);

            throw new SafetyDataQueryException(
                $"Failed to retrieve safety compliance data via {SafetyComplianceProcedureName}.", ex);
        }
    }

    private (int PageNumber, int PageSize) NormalizePaging(SafetySearchCriteria criteria)
    {
        var pageNumber = criteria.PageNumber < 1 ? 1 : criteria.PageNumber;
        var pageSize = criteria.PageSize < 1 ? 50 : Math.Min(criteria.PageSize, _options.MaxPageSize);
        return (pageNumber, pageSize);
    }

    /// <summary>Raw projection of [RiskMgmt].[Risk] before mapping to <see cref="HazardSummaryEntry"/>.</summary>
    private sealed class RiskRow
    {
        public Guid RiskId { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public string HazardSource { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string RiskTitle { get; init; } = string.Empty;
    }

    /// <summary>Raw projection of [RiskMgmt].[ControlMeasure] before grouping by RiskId.</summary>
    private sealed class ControlMeasureRow
    {
        public Guid RiskId { get; init; }
        public string Description { get; init; } = string.Empty;
    }

    /// <summary>Raw projection of [ContinuousImprovement].[CAPA] before mapping to <see cref="CorrectiveActionEntry"/>.</summary>
    private sealed class CapaRow
    {
        public Guid CapaId { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public string ActionType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public DateTime? DueDateUtc { get; init; }
    }

    /// <summary>Raw projection of a dbo.usp_GetSafetyCompliance rollup row before mapping to <see cref="SafetyComplianceEntry"/>.</summary>
    private sealed class SafetyComplianceRow
    {
        public string Category { get; init; } = string.Empty;
        public int TotalCount { get; init; }
        public int OpenCount { get; init; }
        public int ClosedCount { get; init; }
        public int OverdueCount { get; init; }
        public decimal CompliancePercentage { get; init; }
        public DateTime AsOfUtc { get; init; }
    }
}
