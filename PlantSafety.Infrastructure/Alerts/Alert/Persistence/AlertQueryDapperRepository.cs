// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Infrastructure
//  File:           AlertQueryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper read-model repository for the Alert aggregate's DTO queries. All DB
//                  access via stored procedures under the [Alerts] schema.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Domain.Alerts.Alert.Enums;

namespace PlantSafety.Infrastructure.Alerts.Alert.Persistence;

/// <summary>
/// Dapper-backed repository for Alert read-model DTO queries. Executes stored procedures for
/// all persistence operations.
/// </summary>
public sealed class AlertQueryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AlertQueryDapperRepository> logger)
    : IAlertQueryRepository
{
    /// <inheritdoc/>
    public async Task<AlertDto?> GetByIdAsync(Guid tenantId, Guid alertId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById DTO for Alert {AlertId}", alertId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AlertId", alertId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AlertRow>("[Alerts].[usp_Alert_GetByIdDto]", parameters, cancellationToken);
        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlertDto>> GetActiveAlertsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetActiveAlerts for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        var rows = await queryExecutor.QueryAsync<AlertRow>("[Alerts].[usp_Alert_GetActive]", parameters, cancellationToken);
        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlertDto>> GetAlertsByCategoryAsync(Guid tenantId, Guid? siteId, string categoryCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAlertsByCategory {CategoryCode} for tenant {TenantId}", categoryCode, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("CategoryCode", categoryCode, 50)
            .Build();

        var rows = await queryExecutor.QueryAsync<AlertRow>("[Alerts].[usp_Alert_GetByCategory]", parameters, cancellationToken);
        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<AlertDto> Items, int TotalCount)> GetAlertHistoryAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAlertHistory for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        return await QueryHistoryAsync(parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlertDeliveryRecordDto>> GetDeliveryRecordsAsync(Guid alertId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDeliveryRecords for Alert {AlertId}", alertId);

        var parameters = new StoredProcedureBuilder().AddGuid("AlertId", alertId).Build();
        var rows = await queryExecutor.QueryAsync<AlertDeliveryRecordRow>("[Alerts].[usp_Alert_GetDeliveryRecords]", parameters, cancellationToken);

        return rows.Select(r => new AlertDeliveryRecordDto
        {
            DeliveryRecordId = r.DeliveryRecordId,
            AlertId = r.AlertId,
            Channel = r.Channel,
            Succeeded = r.Succeeded,
            AttemptedAtUtc = DateTime.SpecifyKind(r.AttemptedAtUtc, DateTimeKind.Utc),
            FailureReason = r.FailureReason,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlertResponseActionDto>> GetResponseActionsAsync(Guid alertId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetResponseActions for Alert {AlertId}", alertId);

        var parameters = new StoredProcedureBuilder().AddGuid("AlertId", alertId).Build();
        var rows = await queryExecutor.QueryAsync<AlertResponseActionRow>("[Alerts].[usp_Alert_GetResponseActions]", parameters, cancellationToken);

        return rows.Select(r => new AlertResponseActionDto
        {
            ResponseActionId = r.ResponseActionId,
            AlertId = r.AlertId,
            ActionDescription = r.ActionDescription,
            TakenAtUtc = DateTime.SpecifyKind(r.TakenAtUtc, DateTimeKind.Utc),
            TakenBy = r.TakenByActorKind,
            Notes = r.Notes,
        }).ToList();
    }

    // ---- Helpers ---------------------------------------------------------------

    private async Task<(IReadOnlyList<AlertDto> Items, int TotalCount)> QueryHistoryAsync(DynamicParameters parameters, CancellationToken cancellationToken)
    {
        // GetAlertHistoryAsync's stored procedure returns two result sets (TotalCount, then the
        // paginated rows); QueryMultiple is required here, unlike the single-result-set helpers
        // on DapperQueryExecutor, so this method opens its own read-replica-equivalent path via
        // the same executor's connection-scoped semantics by issuing two sequential calls
        // against dedicated, independently-scoped procedures instead of QueryMultiple, keeping
        // strictly within the shared DapperQueryExecutor abstraction (no direct SqlConnection
        // usage here).
        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int>("[Alerts].[usp_Alert_GetHistoryCount]", parameters, cancellationToken);
        var rows = await queryExecutor.QueryAsync<AlertRow>("[Alerts].[usp_Alert_GetHistoryPage]", parameters, cancellationToken);
        return (rows.Select(ToDto).ToList(), totalCount);
    }

    private static AlertDto ToDto(AlertRow row) => new()
    {
        AlertId = row.AlertId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        Status = row.Status,
        CategoryCode = row.CategoryCode,
        CategoryDisplayName = AlertCategory.From(row.CategoryCode).DisplayName,
        SeverityCode = row.SeverityCode,
        SourceType = row.SourceType,
        Title = row.Title,
        LocationZoneName = row.LocationZoneName,
        Latitude = row.LocationLatitude,
        Longitude = row.LocationLongitude,
        TriggeredAtUtc = DateTime.SpecifyKind(row.TriggeredAtUtc, DateTimeKind.Utc),
        AcknowledgedAtUtc = row.AcknowledgedAtUtc.HasValue ? DateTime.SpecifyKind(row.AcknowledgedAtUtc.Value, DateTimeKind.Utc) : null,
        ResolvedAtUtc = row.ResolvedAtUtc.HasValue ? DateTime.SpecifyKind(row.ResolvedAtUtc.Value, DateTimeKind.Utc) : null,
        ClosedAtUtc = row.ClosedAtUtc.HasValue ? DateTime.SpecifyKind(row.ClosedAtUtc.Value, DateTimeKind.Utc) : null,
        CloseOutVerification = row.CloseOutVerification,
    };

    private sealed class AlertRow
    {
        public Guid AlertId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? SiteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string SeverityCode { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LocationZoneName { get; set; } = string.Empty;
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }
        public DateTime TriggeredAtUtc { get; set; }
        public DateTime? AcknowledgedAtUtc { get; set; }
        public DateTime? ResolvedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public string? CloseOutVerification { get; set; }
    }

    private sealed class AlertDeliveryRecordRow
    {
        public Guid DeliveryRecordId { get; set; }
        public Guid AlertId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public bool Succeeded { get; set; }
        public DateTime AttemptedAtUtc { get; set; }
        public string? FailureReason { get; set; }
    }

    private sealed class AlertResponseActionRow
    {
        public Guid ResponseActionId { get; set; }
        public Guid AlertId { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public DateTime TakenAtUtc { get; set; }
        public string TakenByActorKind { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
