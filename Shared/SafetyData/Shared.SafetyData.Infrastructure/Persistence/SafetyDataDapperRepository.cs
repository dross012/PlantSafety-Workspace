// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           SafetyDataDapperRepository.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Read-only implementation of ISafetyDataRepository over
//                  [PlantSafety.CoreDB]'s [RiskMgmt].[Risk] /
//                  [RiskMgmt].[ControlMeasure] / [ContinuousImprovement].[CAPA]
//                  tables, calling dbo.usp_GetHazardSummary,
//                  dbo.usp_GetCorrectiveActions, and dbo.usp_GetSafetyCompliance
//                  (see ..\..\sql\Phase1-StoredProcedures).
//
//                  Phase 4.2 (Repository Modernization): migrated from owning
//                  its own SqlConnection/DynamicParameters directly onto
//                  Shared.SqlGateway's ISqlGateway (Phase 2.6/3.6) -- the
//                  connection, retry/circuit-breaker policy, and structured
//                  SQL telemetry logging are now the gateway's job, not this
//                  repository's. Deferred from Phase 3.6 specifically to
//                  avoid entangling that phase's isolation work with a real
//                  consumer's migration; done now that ISqlGateway itself is
//                  hardened (Phase 3.6's SqlCircuitBreaker + retry policy +
//                  ValidateOnStart).
//
//                  SAFETY: Each method issues a single stored-procedure call that
//                  itself only SELECTs. This class must never gain an
//                  INSERT/UPDATE/DELETE method -- PlantSafety's own aggregates
//                  own all writes to these tables.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using Shared.SafetyData.Abstractions.Models;
using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.Exceptions;
using Shared.SafetyData.Infrastructure.Options;
using Shared.SqlGateway.Abstractions.Gateway;

namespace Shared.SafetyData.Infrastructure.Persistence;

/// <summary>ISqlGateway-backed, read-only repository over PlantSafety's Hazard/Risk, Corrective Action, and Safety Compliance data.</summary>
public sealed class SafetyDataDapperRepository(ISqlGateway sqlGateway, IOptions<SafetyDataOptions> options) : ISafetyDataRepository
{
    private const string HazardSummaryProcedureName = "dbo.usp_GetHazardSummary";
    private const string CorrectiveActionsProcedureName = "dbo.usp_GetCorrectiveActions";
    private const string SafetyComplianceProcedureName = "dbo.usp_GetSafetyCompliance";

    private readonly SafetyDataOptions _options = options.Value;

    /// <inheritdoc/>
    public async Task<SafetySearchResult<HazardSummaryEntry>> GetHazardSummaryAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var (pageNumber, pageSize) = NormalizePaging(criteria);
        var parameters = new
        {
            TenantId = criteria.TenantId,
            SiteId = criteria.SiteId,
            FromUtc = criteria.FromUtc,
            ToUtc = criteria.ToUtc,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        try
        {
            // dbo.usp_GetHazardSummary returns three result sets: (1) a single-row total
            // count, (2) the requested page of RiskMgmt.Risk rows, newest first, (3) the
            // RiskMgmt.ControlMeasure rows for every RiskId in result set 2 -- see the
            // stored procedure's header comment for why control measures are returned as
            // a separate result set rather than a joined/duplicated row set.
            await using ISqlMultiResultReader multi = await sqlGateway.QueryMultipleAsync(HazardSummaryProcedureName, parameters, cancellationToken);

            var totalCount = await multi.ReadFirstAsync<int>();
            IReadOnlyList<RiskRow> riskRows = await multi.ReadAsync<RiskRow>();
            IReadOnlyList<ControlMeasureRow> controlMeasureRows = await multi.ReadAsync<ControlMeasureRow>();

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
            throw new SafetyDataQueryException(
                $"Failed to retrieve hazard summary data via {HazardSummaryProcedureName}.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SafetySearchResult<CorrectiveActionEntry>> GetCorrectiveActionsAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var (pageNumber, pageSize) = NormalizePaging(criteria);
        var parameters = new
        {
            TenantId = criteria.TenantId,
            SiteId = criteria.SiteId,
            FromUtc = criteria.FromUtc,
            ToUtc = criteria.ToUtc,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        try
        {
            // dbo.usp_GetCorrectiveActions returns two result sets: (1) a single-row total
            // count, (2) the requested page of ContinuousImprovement.CAPA rows, newest first.
            await using ISqlMultiResultReader multi = await sqlGateway.QueryMultipleAsync(CorrectiveActionsProcedureName, parameters, cancellationToken);

            var totalCount = await multi.ReadFirstAsync<int>();
            IReadOnlyList<CapaRow> rows = await multi.ReadAsync<CapaRow>();

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
            throw new SafetyDataQueryException(
                $"Failed to retrieve corrective action data via {CorrectiveActionsProcedureName}.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyComplianceEntry>> GetSafetyComplianceAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var parameters = new
        {
            TenantId = criteria.TenantId,
            SiteId = criteria.SiteId,
            FromUtc = criteria.FromUtc,
            ToUtc = criteria.ToUtc,
        };

        try
        {
            IReadOnlyList<SafetyComplianceRow> rows = await sqlGateway.QueryAsync<SafetyComplianceRow>(SafetyComplianceProcedureName, parameters, cancellationToken);

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
