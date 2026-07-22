// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Dapper repository for the Dashboard aggregate. Implements both the Domain repository
//      contract (PlantSafety.Domain.Dashboards.Dashboard.Interfaces.IDashboardRepository) and the
//      Application persistence + read-model contract
//      (PlantSafety.Application.Dashboards.Dashboard.Repositories.IDashboardRepository). All DB
//      access is via stored procedures under the [Dashboards] schema (distinct from, and never
//      touching, the pre-existing [Analytics].[Dashboard] table/procedures owned by the
//      Analytics.Dashboard "canonical reference module").
//
//      REMARK -- Domain interface tenant-scoping gap (flagged, not silently worked around):
//      IDashboardRepository (Domain).GetByIdAsync/ListAsync carry no TenantId parameter, but
//      every tenant-scoped table in this platform is protected by a strict, deny-by-default
//      Row-Level-Security policy keyed on SESSION_CONTEXT('TenantId') (see
//      90-row-level-security.sql). Per the governance instruction to apply the stricter
//      compliance rule and never loosen a control to make a check pass, these two methods do
//      not set a tenant session context (none is available), so under the platform's default
//      security policy they will return no rows unless invoked on a connection/transaction
//      where SESSION_CONTEXT('TenantId') has already been established by an outer
//      Application-layer call in the same unit of work. This is a pre-existing Domain-interface
//      design gap, not introduced here.
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Dashboards.Dashboard.Models;
using PlantSafety.Domain.Dashboards.Dashboard.Entities;
using PlantSafety.Domain.Dashboards.Dashboard.Enums;
using PlantSafety.Domain.Dashboards.Dashboard.Specifications;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Persistence;
using DomainDashboard = PlantSafety.Domain.Dashboards.Dashboard.Aggregates.Dashboard;
using DomainIDashboardRepository = PlantSafety.Domain.Dashboards.Dashboard.Interfaces.IDashboardRepository;
using AppIDashboardRepository = PlantSafety.Application.Dashboards.Dashboard.Repositories.IDashboardRepository;
using WidgetId = PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId;

namespace PlantSafety.Infrastructure.Dashboards.Dashboard.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="DomainDashboard"/> aggregate. Executes stored
/// procedures under the <c>[Dashboards]</c> schema for all persistence operations.
/// </summary>
public sealed class DashboardDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DashboardDapperRepository> logger)
    : DomainIDashboardRepository, AppIDashboardRepository
{
    /// <inheritdoc cref="DomainIDashboardRepository.GetByIdAsync"/>
    public async Task<DomainDashboard?> GetByIdAsync(DashboardId dashboardId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for Dashboard {DashboardId}", dashboardId.Value);

        var row = await queryExecutor.QuerySingleOrDefaultAsync<DashboardRow>(
            "[Dashboards].[usp_Dashboard_GetById]", new StoredProcedureBuilder().AddGuid("DashboardId", dashboardId.Value).Build(), cancellationToken);
        if (row is null) return null;

        return await RehydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DomainDashboard>> ListAsync(DashboardSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Dashboard (in-memory specification filter)");

        var rows = await queryExecutor.QueryAsync<DashboardRow>("[Dashboards].[usp_Dashboard_ListAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        var result = new List<DomainDashboard>();

        foreach (var row in rows)
        {
            var aggregate = await RehydrateAsync(row, cancellationToken);
            if (predicate(aggregate))
                result.Add(aggregate);
        }

        return result;
    }

    /// <inheritdoc cref="DomainIDashboardRepository.AddAsync"/>
    public async Task AddAsync(DomainDashboard aggregate, CancellationToken cancellationToken)
    {
        logger.LogDebug("Insert Dashboard {DashboardId} for tenant {TenantId}", aggregate.DashboardId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = aggregate.CreatedBy.Value;
        var nowUtc = DateTime.UtcNow;

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DashboardId", aggregate.DashboardId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Name", aggregate.Name, 200)
            .AddString("Description", aggregate.Description, 2000)
            .AddString("CategoryCode", aggregate.Category.Code, 50)
            .AddString("Role", aggregate.Role.ToString(), 30)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_Create]", parameters, tx, cancellationToken: cancellationToken);

        foreach (var filter in aggregate.DefaultFilters)
        {
            var filterParameters = new StoredProcedureBuilder()
                .AddGuid("DashboardId", aggregate.DashboardId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("FilterKey", filter.Key, 100)
                .AddString("FilterValue", filter.Value, 500)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_AddDefaultFilter]", filterParameters, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc cref="DomainIDashboardRepository.UpdateAsync"/>
    public async Task UpdateAsync(DomainDashboard aggregate, CancellationToken cancellationToken)
    {
        logger.LogDebug("Update Dashboard {DashboardId} for tenant {TenantId}", aggregate.DashboardId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = aggregate.CreatedBy.Value;
        var nowUtc = DateTime.UtcNow;

        // Status transition.
        var statusParameters = new StoredProcedureBuilder()
            .AddGuid("DashboardId", aggregate.DashboardId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
            .Build();
        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_UpdateStatus]", statusParameters, tx, cancellationToken: cancellationToken);

        // Default filters: replace wholesale (SetDefaultFilters is the only mutator and is a
        // full replace on the in-memory aggregate too).
        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_ClearDefaultFilters]",
            new StoredProcedureBuilder().AddGuid("DashboardId", aggregate.DashboardId.Value).AddGuid("TenantId", aggregate.TenantId).Build(),
            tx, cancellationToken: cancellationToken);

        foreach (var filter in aggregate.DefaultFilters)
        {
            var filterParameters = new StoredProcedureBuilder()
                .AddGuid("DashboardId", aggregate.DashboardId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("FilterKey", filter.Key, 100)
                .AddString("FilterValue", filter.Value, 500)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_AddDefaultFilter]", filterParameters, tx, cancellationToken: cancellationToken);
        }

        // Widget layout: additive placements are inserted here; removals are handled
        // immediately by RemoveWidget via a dedicated delete procedure (see below), so only
        // new placements need to be synced on Update.
        var persistedPlacementWidgetIds = (await queryExecutor.QueryAsync<DashboardWidgetPlacementRow>(
                "[Dashboards].[usp_Dashboard_GetWidgetLayout]", new StoredProcedureBuilder().AddGuid("DashboardId", aggregate.DashboardId.Value).Build(), cancellationToken))
            .Select(p => p.WidgetId).ToHashSet();

        foreach (var placement in aggregate.WidgetLayout.Where(p => !persistedPlacementWidgetIds.Contains(p.WidgetId.Value)))
        {
            var placementParameters = new StoredProcedureBuilder()
                .AddGuid("PlacementId", placement.PlacementId)
                .AddGuid("DashboardId", aggregate.DashboardId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("WidgetId", placement.WidgetId.Value)
                .AddInt("Row", placement.Row)
                .AddInt("Column", placement.Column)
                .AddInt("RowSpan", placement.RowSpan)
                .AddInt("ColumnSpan", placement.ColumnSpan)
                .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_AddWidget]", placementParameters, tx, cancellationToken: cancellationToken);

            foreach (var filterOverride in placement.FilterOverrides)
            {
                var overrideParameters = new StoredProcedureBuilder()
                    .AddGuid("PlacementId", placement.PlacementId)
                    .AddGuid("TenantId", aggregate.TenantId)
                    .AddString("FilterKey", filterOverride.Key, 100)
                    .AddString("FilterValue", filterOverride.Value, 500)
                    .Build();

                await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_AddWidgetPlacementFilterOverride]", overrideParameters, tx, cancellationToken: cancellationToken);
            }
        }

        // Removed placements: any persisted placement no longer present in the in-memory layout.
        var currentWidgetIds = aggregate.WidgetLayout.Select(p => p.WidgetId.Value).ToHashSet();
        foreach (var removedWidgetId in persistedPlacementWidgetIds.Where(id => !currentWidgetIds.Contains(id)))
        {
            var removeParameters = new StoredProcedureBuilder()
                .AddGuid("DashboardId", aggregate.DashboardId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("WidgetId", removedWidgetId)
                .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Dashboard_RemoveWidget]", removeParameters, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<DashboardDto?> GetDtoByIdAsync(Guid tenantId, Guid dashboardId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDtoById for Dashboard {DashboardId}", dashboardId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("DashboardId", dashboardId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DashboardRow>("[Dashboards].[usp_Dashboard_GetByIdDto]", parameters, cancellationToken);
        if (row is null) return null;

        return await ToDtoAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DashboardDto>> GetDashboardsForRoleAsync(Guid tenantId, string viewerRole, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDashboardsForRole {Role} for tenant {TenantId}", viewerRole, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddString("Role", viewerRole, 30).Build();
        var rows = await queryExecutor.QueryAsync<DashboardRow>("[Dashboards].[usp_Dashboard_GetForRole]", parameters, cancellationToken);

        var result = new List<DashboardDto>();
        foreach (var row in rows)
            result.Add(await ToDtoAsync(row, cancellationToken));

        return result;
    }

    // ---- Rehydration / mapping -------------------------------------------------

    private async Task<DomainDashboard> RehydrateAsync(DashboardRow row, CancellationToken cancellationToken)
    {
        var dashboard = AggregateReflectionRehydrator.CreateUninitialized<DomainDashboard>();

        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.DashboardId), new DashboardId(row.DashboardId));
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.TenantId), row.TenantId);
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.SiteId), row.SiteId);
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.Status), Enum.Parse<DashboardStatus>(row.Status));
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.Name), row.Name);
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.Description), row.Description);
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.Category), DashboardCategory.From(row.CategoryCode));
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.Role), Enum.Parse<DashboardRole>(row.Role));
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionRehydrator.SetProperty(dashboard, nameof(DomainDashboard.CreatedBy), UserId.FromGuid(row.CreatedBy));

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Alerts.Alert aggregate): RuntimeHelpers.GetUninitializedObject bypasses ALL
        // field initializers, so these readonly list-backed fields start out null, not the empty
        // list their inline initializers declare. GetListField only reads the current field
        // value (it does not create one), so Add() below -- or any public collection getter --
        // would NullReferenceException without this.
        AggregateReflectionRehydrator.SetField(dashboard, "_defaultFilters", new List<DashboardFilter>());
        AggregateReflectionRehydrator.SetField(dashboard, "_widgetLayout", new List<DashboardWidgetPlacement>());

        var filterRows = await queryExecutor.QueryAsync<DashboardDefaultFilterRow>(
            "[Dashboards].[usp_Dashboard_GetDefaultFilters]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);
        var filterList = AggregateReflectionRehydrator.GetListField<DomainDashboard, DashboardFilter>(dashboard, "_defaultFilters");
        foreach (var f in filterRows)
            filterList.Add(DashboardFilter.Create(f.FilterKey, f.FilterValue));

        var placementRows = await queryExecutor.QueryAsync<DashboardWidgetPlacementRow>(
            "[Dashboards].[usp_Dashboard_GetWidgetLayout]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);
        var overrideRows = await queryExecutor.QueryAsync<DashboardWidgetPlacementFilterOverrideRow>(
            "[Dashboards].[usp_Dashboard_GetPlacementFilterOverrides]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);

        var placementList = AggregateReflectionRehydrator.GetListField<DomainDashboard, DashboardWidgetPlacement>(dashboard, "_widgetLayout");
        foreach (var p in placementRows)
        {
            var overrides = overrideRows.Where(o => o.PlacementId == p.PlacementId)
                .Select(o => DashboardFilter.Create(o.FilterKey, o.FilterValue)).ToList();

            placementList.Add(new DashboardWidgetPlacement(
                p.PlacementId, new DashboardId(p.DashboardId), new WidgetId(p.WidgetId), p.Row, p.Column, p.RowSpan, p.ColumnSpan, overrides));
        }

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Alerts.Alert aggregate, which shares this same AggregateReflectionRehydrator
        // helper): _domainEvents was never initialized here, so any lifecycle-transition method
        // calling AddDomainEvent() on a rehydrated aggregate (e.g. Publish()/Archive()) would
        // throw NullReferenceException.
        AggregateReflectionRehydrator.SetField(dashboard, "_domainEvents", new List<PlantSafety.Domain.Dashboards.Dashboard.Events.IDashboardDomainEvent>());

        return dashboard;
    }

    private async Task<DashboardDto> ToDtoAsync(DashboardRow row, CancellationToken cancellationToken)
    {
        var filterRows = await queryExecutor.QueryAsync<DashboardDefaultFilterRow>(
            "[Dashboards].[usp_Dashboard_GetDefaultFilters]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);
        var placementRows = await queryExecutor.QueryAsync<DashboardWidgetPlacementRow>(
            "[Dashboards].[usp_Dashboard_GetWidgetLayout]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);
        var overrideRows = await queryExecutor.QueryAsync<DashboardWidgetPlacementFilterOverrideRow>(
            "[Dashboards].[usp_Dashboard_GetPlacementFilterOverrides]", new StoredProcedureBuilder().AddGuid("DashboardId", row.DashboardId).Build(), cancellationToken);

        return new DashboardDto
        {
            DashboardId = row.DashboardId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            Status = row.Status,
            Name = row.Name,
            Description = row.Description,
            CategoryCode = row.CategoryCode,
            Role = row.Role,
            DefaultFilters = filterRows.ToDictionary(f => f.FilterKey, f => f.FilterValue),
            WidgetLayout = placementRows.Select(p => new DashboardWidgetPlacementDto
            {
                PlacementId = p.PlacementId,
                WidgetId = p.WidgetId,
                Row = p.Row,
                Column = p.Column,
                RowSpan = p.RowSpan,
                ColumnSpan = p.ColumnSpan,
                FilterOverrides = overrideRows.Where(o => o.PlacementId == p.PlacementId).ToDictionary(o => o.FilterKey, o => o.FilterValue),
            }).ToList(),
        };
    }

    // ---- Dapper row-mapping POCOs ---------------------------------------------

    private sealed class DashboardRow
    {
        public Guid DashboardId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? SiteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
    }

    private sealed class DashboardDefaultFilterRow
    {
        public Guid DashboardId { get; set; }
        public string FilterKey { get; set; } = string.Empty;
        public string FilterValue { get; set; } = string.Empty;
    }

    private sealed class DashboardWidgetPlacementRow
    {
        public Guid PlacementId { get; set; }
        public Guid DashboardId { get; set; }
        public Guid WidgetId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
    }

    private sealed class DashboardWidgetPlacementFilterOverrideRow
    {
        public Guid PlacementId { get; set; }
        public string FilterKey { get; set; } = string.Empty;
        public string FilterValue { get; set; } = string.Empty;
    }
}
