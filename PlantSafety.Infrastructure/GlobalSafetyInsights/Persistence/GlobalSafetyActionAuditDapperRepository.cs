// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyActionAuditDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IGlobalSafetyActionAuditRepository, writing to
//                  [GlobalSafetyInsights].[CrossSiteActionLog]. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;
using PlantSafety.Infrastructure.Common.Serialization;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Persistence;

/// <summary>Dapper-backed audit ledger for cross-site coordinated actions.</summary>
public sealed class GlobalSafetyActionAuditDapperRepository(
    ISqlConnectionFactory connectionFactory, DapperQueryExecutor queryExecutor, DapperCommandExecutor commandExecutor, ILogger<GlobalSafetyActionAuditDapperRepository> logger)
    : IGlobalSafetyActionAuditRepository
{
    /// <inheritdoc/>
    public async Task RecordAsync(
        Guid tenantId, IReadOnlyList<Guid> affectedSiteIds, string actionType, string description,
        Guid? initiatedByUserId, DateTime recordedAtUtc, string? regionCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("Recording CrossSiteAction '{ActionType}' for Tenant {TenantId}", actionType, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CrossSiteActionLogId", Guid.NewGuid())
            .AddGuid("TenantId", tenantId)
            .AddString("AffectedSiteIdsJson", JsonSerializer.Serialize(affectedSiteIds, JsonSerializerConfiguration.Default))
            .AddString("ActionType", actionType, 100)
            .AddString("Description", description, 2000)
            .AddGuid("InitiatedByUserId", initiatedByUserId)
            .AddDateTime("RecordedAtUtc", recordedAtUtc)
            .AddString("RegionCode", regionCode, 20)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(conn, "[GlobalSafetyInsights].[usp_CrossSiteActionLog_Insert]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CrossSiteActionLogEntryDto>> ListAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("Listing CrossSiteAction history for Tenant {TenantId} [{From:u}..{To:u}]", tenantId, from, to);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        var rows = await queryExecutor.QueryAsync<CrossSiteActionLogRow>(
            "[GlobalSafetyInsights].[usp_CrossSiteActionLog_ListByTenant]", parameters, cancellationToken);

        return rows.Select(row => new CrossSiteActionLogEntryDto
        {
            CrossSiteActionLogId = row.CrossSiteActionLogId,
            TenantId = row.TenantId,
            AffectedSiteIds = JsonSerializer.Deserialize<List<Guid>>(row.AffectedSiteIdsJson, JsonSerializerConfiguration.Default) ?? [],
            ActionType = row.ActionType,
            Description = row.Description ?? string.Empty,
            InitiatedByUserId = row.InitiatedByUserId,
            RecordedAtUtc = row.RecordedAtUtc,
            RegionCode = row.RegionCode,
        }).ToList();
    }

    /// <summary>Flat read-model row shape returned by usp_CrossSiteActionLog_ListByTenant.</summary>
    private sealed record CrossSiteActionLogRow
    {
        public required Guid CrossSiteActionLogId { get; init; }
        public required Guid TenantId { get; init; }
        public required string AffectedSiteIdsJson { get; init; }
        public required string ActionType { get; init; }
        public string? Description { get; init; }
        public Guid? InitiatedByUserId { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public string? RegionCode { get; init; }
    }
}
