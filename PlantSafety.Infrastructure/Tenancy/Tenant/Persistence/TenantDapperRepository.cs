// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ITenantRepository. All DB access via stored
//                  procedures in the [Tenancy] schema. Unlike every other tenant-scoped
//                  repository in this codebase, calls here are NOT scoped by an incoming
//                  @TenantId parameter representing "which tenant's data this belongs to" --
//                  the Tenant row's own TenantId IS the entity being read/written (see
//                  docs/Phase7/tenant-isolation-model.md for why the Tenancy.Tenant table itself
//                  is excluded from the platform's Row-Level-Security policy). Additive-only
//                  evolution.
//
//                  ASSUMPTION FLAGGED: GetByNameAsync/NameExistsAsync/ListAllAsync legitimately
//                  issue no @TenantId parameter (there is no single tenant to scope a
//                  cross-tenant registry lookup/listing by). TenantContextPropagationOptions
//                  .EnableTenantContextPropagation (default OFF) is a platform-wide staged
//                  rollout gate that, once turned ON, requires every DapperQueryExecutor/
//                  DapperCommandExecutor call to carry a non-empty TenantId or throw
//                  TenantContextRequiredException -- these three calls are a deliberate,
//                  permanent exception to that gate (the Tenant registry itself, not a
//                  tenant-scoped business record), analogous to the existing AuditLog/ErrorLog/
//                  ArchivalJob exclusions from the SQL Row-Level-Security policy. If that flag is
//                  ever enabled platform-wide, these three call sites will need an explicit
//                  allow-list entry rather than being treated as an unmigrated gap.
// ============================================================================
using System.Text.Json;
using PlantSafety.Domain.Tenancy.Tenant.Entities;
using PlantSafety.Domain.Tenancy.Tenant.Enums;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using TenantAggregate = PlantSafety.Domain.Tenancy.Tenant.Aggregates.Tenant;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Persistence;

/// <summary>Dapper-backed repository for the <see cref="TenantAggregate"/> aggregate root.</summary>
public sealed class TenantDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TenantDapperRepository> logger)
    : ITenantRepository
{
    /// <inheritdoc/>
    public async Task<TenantAggregate?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<TenantRow>("[Tenancy].[usp_Tenant_GetById]", parameters, cancellationToken);
        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<TenantAggregate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddString("Name", name, 200).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<TenantRow>("[Tenancy].[usp_Tenant_GetByName]", parameters, cancellationToken);
        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddString("Name", name, 200).Build();
        var count = await queryExecutor.QuerySingleOrDefaultAsync<int>("[Tenancy].[usp_Tenant_NameExists]", parameters, cancellationToken);
        return count > 0;
    }

    /// <inheritdoc/>
    public async Task AddAsync(TenantAggregate tenant, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Tenant {TenantId}", tenant.TenantId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildUpsertParameters(tenant);
        await commandExecutor.ExecuteAsync(conn, "[Tenancy].[usp_Tenant_Insert]", parameters, tx, cancellationToken: cancellationToken);

        tx.Commit();

        FlushDomainEvents(tenant);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TenantAggregate tenant, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Tenant {TenantId} Status {Status}", tenant.TenantId.Value, tenant.Status.Code);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildUpsertParameters(tenant);
        await commandExecutor.ExecuteAsync(conn, "[Tenancy].[usp_Tenant_Update]", parameters, tx, cancellationToken: cancellationToken);

        tx.Commit();

        FlushDomainEvents(tenant);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TenantAggregate>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<TenantRow>("[Tenancy].[usp_Tenant_ListAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        return rows.Select(Hydrate).ToList();
    }

    private void FlushDomainEvents(TenantAggregate tenant)
    {
        foreach (var domainEvent in tenant.DomainEvents)
            outboxWriter.Enqueue(domainEvent, tenant.TenantId.Value.ToString(), "system", Guid.Empty.ToString());
        tenant.ClearDomainEvents();
    }

    private static StoredProcedureBuilder BuildUpsertParametersBase(TenantAggregate tenant) => new StoredProcedureBuilder()
        .AddGuid("TenantId", tenant.TenantId.Value)
        .AddString("Name", tenant.Name, 200)
        .AddString("Region", tenant.Region, 50)
        .AddString("Status", tenant.Status.Code, 20)
        .AddString("PlanCode", tenant.PlanCode.Code, 30)
        .AddString("ConfigurationJson", SerializeConfiguration(tenant.Configuration), size: -1)
        .AddDateTime("CreatedAtUtc", tenant.CreatedAtUtc.Value)
        .AddDateTime("ActivatedAtUtc", tenant.ActivatedAtUtc?.Value)
        .AddDateTime("SuspendedAtUtc", tenant.SuspendedAtUtc?.Value)
        .AddString("SuspensionReason", tenant.SuspensionReason, 2000)
        .AddDateTime("ArchivedAtUtc", tenant.ArchivedAtUtc?.Value)
        .AddDateTime("DeletedAtUtc", tenant.DeletedAtUtc?.Value)
        .AddString("DeletionReason", tenant.DeletionReason, 2000);

    private static DynamicParameters BuildUpsertParameters(TenantAggregate tenant) => BuildUpsertParametersBase(tenant).Build();

    private static string SerializeConfiguration(TenantConfiguration configuration) => JsonSerializer.Serialize(new TenantConfigurationRow(
        configuration.EnabledModules.ToList(),
        configuration.FeatureFlagOverrides.Select(f => new TenantFeatureFlagOverrideRow(f.FlagName, f.IsEnabled)).ToList(),
        configuration.CustomComplianceRuleCodes.ToList()));

    private static TenantAggregate Hydrate(TenantRow row)
    {
        var configRow = string.IsNullOrWhiteSpace(row.ConfigurationJson)
            ? new TenantConfigurationRow([], [], [])
            : JsonSerializer.Deserialize<TenantConfigurationRow>(row.ConfigurationJson) ?? new TenantConfigurationRow([], [], []);

        var configuration = TenantConfiguration.Rehydrate(
            configRow.EnabledModules,
            configRow.FeatureFlagOverrides.Select(f => new TenantFeatureFlagOverride(f.FlagName, f.IsEnabled)),
            configRow.CustomComplianceRuleCodes);

        return TenantAggregate.Rehydrate(
            TenantId.FromGuid(row.TenantId),
            row.Name,
            row.Region,
            TenantStatus.From(row.Status),
            SubscriptionPlanCode.From(row.PlanCode),
            configuration,
            UtcTimestamp.From(row.CreatedAtUtc),
            row.ActivatedAtUtc.HasValue ? UtcTimestamp.From(row.ActivatedAtUtc.Value) : null,
            row.SuspendedAtUtc.HasValue ? UtcTimestamp.From(row.SuspendedAtUtc.Value) : null,
            row.SuspensionReason,
            row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : null,
            row.DeletedAtUtc.HasValue ? UtcTimestamp.From(row.DeletedAtUtc.Value) : null,
            row.DeletionReason);
    }

    private sealed record TenantRow
    {
        public required Guid TenantId { get; init; }
        public required string Name { get; init; }
        public required string Region { get; init; }
        public required string Status { get; init; }
        public required string PlanCode { get; init; }
        public string? ConfigurationJson { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? ActivatedAtUtc { get; init; }
        public DateTime? SuspendedAtUtc { get; init; }
        public string? SuspensionReason { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public DateTime? DeletedAtUtc { get; init; }
        public string? DeletionReason { get; init; }
    }

    private sealed record TenantConfigurationRow(
        List<string> EnabledModules,
        List<TenantFeatureFlagOverrideRow> FeatureFlagOverrides,
        List<string> CustomComplianceRuleCodes);

    private sealed record TenantFeatureFlagOverrideRow(string FlagName, bool IsEnabled);
}
