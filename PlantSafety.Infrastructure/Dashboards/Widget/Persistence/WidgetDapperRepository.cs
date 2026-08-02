// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Infrastructure
//  File:           WidgetDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Dapper repository for the Widget aggregate. Implements both the Domain repository
//      contract (PlantSafety.Domain.Dashboards.Widget.Interfaces.IWidgetRepository) and the
//      Application persistence + read-model contract
//      (PlantSafety.Application.Dashboards.Widget.Repositories.IWidgetRepository). All DB access
//      is via stored procedures under the [Dashboards] schema.
//
//      REMARK -- Domain interface tenant-scoping gap (flagged, not silently worked around): see
//      the identical remark on DashboardDapperRepository; the same strict RLS consequence
//      applies to IWidgetRepository (Domain).GetByIdAsync/ListAsync.
//
//      REMARK -- GetWidgetDataAsync cross-module data resolution (flagged, not silently
//      absorbed): a Widget's WidgetDataSource points at an arbitrary OTHER module's
//      query/endpoint (e.g. Alerts.GetActiveAlertsQuery). Wiring a generic per-widget resolver
//      here would require this Infrastructure module to reference every one of the platform's
//      60+ module Application/Infrastructure assemblies, which would invert the intended
//      module boundary (Dashboards should not depend on every other module). No existing
//      cross-module "widget data adapter" abstraction exists elsewhere in the codebase to plug
//      into. This method therefore validates the widget exists for the tenant and returns a
//      deterministic, empty WidgetDataContractDto (Series/Categories/Metrics all empty) rather
//      than fabricating adapter wiring or throwing NotImplementedException. Implementing real
//      cross-module data resolution is flagged as required follow-up work, most naturally as a
//      dedicated per-source-module adapter registry resolved via DI keyed on
//      WidgetDataSource.SourceModule.
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.Specifications;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Persistence;
using DomainWidget = PlantSafety.Domain.Dashboards.Widget.Aggregates.Widget;
using DomainIWidgetRepository = PlantSafety.Domain.Dashboards.Widget.Interfaces.IWidgetRepository;
using AppIWidgetRepository = PlantSafety.Application.Dashboards.Widget.Repositories.IWidgetRepository;

namespace PlantSafety.Infrastructure.Dashboards.Widget.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="DomainWidget"/> aggregate. Executes stored
/// procedures under the <c>[Dashboards]</c> schema for all persistence operations.
/// </summary>
public sealed class WidgetDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WidgetDapperRepository> logger)
    : DomainIWidgetRepository, AppIWidgetRepository
{
    /// <inheritdoc cref="DomainIWidgetRepository.GetByIdAsync"/>
    public async Task<DomainWidget?> GetByIdAsync(WidgetId widgetId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for Widget {WidgetId}", widgetId.Value);

        var row = await queryExecutor.QuerySingleOrDefaultAsync<WidgetRow>(
            "[Dashboards].[usp_Widget_GetById]", new StoredProcedureBuilder().AddGuid("WidgetId", widgetId.Value).Build(), cancellationToken);
        if (row is null) return null;

        return await RehydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DomainWidget>> ListAsync(WidgetSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Widget (in-memory specification filter)");

        var rows = await queryExecutor.QueryAsync<WidgetRow>("[Dashboards].[usp_Widget_ListAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        var result = new List<DomainWidget>();

        foreach (var row in rows)
        {
            var aggregate = await RehydrateAsync(row, cancellationToken);
            if (predicate(aggregate))
                result.Add(aggregate);
        }

        return result;
    }

    /// <inheritdoc cref="DomainIWidgetRepository.AddAsync"/>
    public async Task AddAsync(DomainWidget aggregate, CancellationToken cancellationToken)
    {
        logger.LogDebug("Insert Widget {WidgetId} for tenant {TenantId}", aggregate.WidgetId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = aggregate.CreatedBy.Value;
        var nowUtc = DateTime.UtcNow;

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WidgetId", aggregate.WidgetId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Name", aggregate.Name, 200)
            .AddString("TypeCode", aggregate.Type.Code, 30)
            .AddString("DataSourceModule", aggregate.DataSource.SourceModule, 100)
            .AddString("DataSourceQueryOrEndpoint", aggregate.DataSource.QueryOrEndpoint, 200)
            .AddInt("RefreshIntervalSeconds", aggregate.RefreshInterval.Seconds)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_Create]", parameters, tx, cancellationToken: cancellationToken);

        foreach (var parameter in aggregate.Parameters)
        {
            var paramParameters = new StoredProcedureBuilder()
                .AddGuid("WidgetId", aggregate.WidgetId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("ParamKey", parameter.Key, 100)
                .AddString("ParamValue", parameter.Value, 500)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_AddParameter]", paramParameters, tx, cancellationToken: cancellationToken);
        }

        foreach (var role in aggregate.PermittedRoles)
        {
            var roleParameters = new StoredProcedureBuilder()
                .AddGuid("WidgetId", aggregate.WidgetId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("RoleName", role, 50)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_AddPermittedRole]", roleParameters, tx, cancellationToken: cancellationToken);
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

    /// <inheritdoc cref="DomainIWidgetRepository.UpdateAsync"/>
    public async Task UpdateAsync(DomainWidget aggregate, CancellationToken cancellationToken)
    {
        logger.LogDebug("Update Widget {WidgetId} for tenant {TenantId}", aggregate.WidgetId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = aggregate.CreatedBy.Value;
        var nowUtc = DateTime.UtcNow;

        // Status transition (Activate/Deprecate).
        if (aggregate.Status == WidgetStatus.Active)
        {
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_Activate]",
                new StoredProcedureBuilder().AddGuid("WidgetId", aggregate.WidgetId.Value).AddGuid("TenantId", aggregate.TenantId)
                    .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc).Build(),
                tx, cancellationToken: cancellationToken);
        }
        else if (aggregate.Status == WidgetStatus.Deprecated)
        {
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_Deprecate]",
                new StoredProcedureBuilder().AddGuid("WidgetId", aggregate.WidgetId.Value).AddGuid("TenantId", aggregate.TenantId)
                    .AddString("DeprecationReason", "Deprecated", 500)
                    .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc).Build(),
                tx, cancellationToken: cancellationToken);
        }

        // Configuration (data source / refresh interval): always reflect current in-memory state.
        var configureParameters = new StoredProcedureBuilder()
            .AddGuid("WidgetId", aggregate.WidgetId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("DataSourceModule", aggregate.DataSource.SourceModule, 100)
            .AddString("DataSourceQueryOrEndpoint", aggregate.DataSource.QueryOrEndpoint, 200)
            .AddInt("RefreshIntervalSeconds", aggregate.RefreshInterval.Seconds)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
            .Build();
        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_Configure]", configureParameters, tx, cancellationToken: cancellationToken);

        // Parameters: full replace (Configure() clears+re-adds on the in-memory aggregate too).
        await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_ClearParameters]",
            new StoredProcedureBuilder().AddGuid("WidgetId", aggregate.WidgetId.Value).AddGuid("TenantId", aggregate.TenantId).Build(),
            tx, cancellationToken: cancellationToken);

        foreach (var parameter in aggregate.Parameters)
        {
            var paramParameters = new StoredProcedureBuilder()
                .AddGuid("WidgetId", aggregate.WidgetId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("ParamKey", parameter.Key, 100)
                .AddString("ParamValue", parameter.Value, 500)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[Dashboards].[usp_Widget_AddParameter]", paramParameters, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task<WidgetDto?> GetDtoByIdAsync(Guid tenantId, Guid widgetId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDtoById for Widget {WidgetId}", widgetId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("WidgetId", widgetId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WidgetRow>("[Dashboards].[usp_Widget_GetByIdDto]", parameters, cancellationToken);
        if (row is null) return null;

        return await ToDtoAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WidgetDto>> GetWidgetLibraryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWidgetLibrary for tenant {TenantId}", tenantId);

        var rows = await queryExecutor.QueryAsync<WidgetRow>(
            "[Dashboards].[usp_Widget_GetLibrary]", new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build(), cancellationToken);

        var result = new List<WidgetDto>();
        foreach (var row in rows)
            result.Add(await ToDtoAsync(row, cancellationToken));

        return result;
    }

    /// <inheritdoc/>
    public async Task<WidgetDataContractDto> GetWidgetDataAsync(
        Guid tenantId, Guid widgetId, IReadOnlyDictionary<string, string> filterOverrides, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWidgetData for Widget {WidgetId} and tenant {TenantId}", widgetId, tenantId);

        var existsParameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("WidgetId", widgetId).Build();
        var count = await queryExecutor.QuerySingleOrDefaultAsync<int>("[Dashboards].[usp_Widget_Exists]", existsParameters, cancellationToken);

        if (count <= 0)
        {
            logger.LogWarning("GetWidgetData: Widget {WidgetId} not found for tenant {TenantId}.", widgetId, tenantId);
        }

        // See class-level remark: real cross-module data resolution requires a per-source-module
        // adapter that does not yet exist anywhere in the codebase. Returning a deterministic,
        // empty contract rather than fabricating one.
        return new WidgetDataContractDto
        {
            WidgetId = widgetId,
            ResolvedAtUtc = DateTime.UtcNow,
            Series = Array.Empty<WidgetDataSeriesDto>(),
            Categories = Array.Empty<WidgetDataCategoryDto>(),
            Metrics = Array.Empty<WidgetDataMetricDto>(),
        };
    }

    // ---- Rehydration / mapping -------------------------------------------------

    private async Task<DomainWidget> RehydrateAsync(WidgetRow row, CancellationToken cancellationToken)
    {
        var widget = AggregateReflectionRehydrator.CreateUninitialized<DomainWidget>();

        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.WidgetId), new WidgetId(row.WidgetId));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.TenantId), row.TenantId);
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.Status), Enum.Parse<WidgetStatus>(row.Status));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.Name), row.Name);
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.Type), WidgetType.From(row.TypeCode));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.DataSource), WidgetDataSource.Create(row.DataSourceModule, row.DataSourceQueryOrEndpoint));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.RefreshInterval), WidgetRefreshInterval.FromSeconds(row.RefreshIntervalSeconds));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionRehydrator.SetProperty(widget, nameof(DomainWidget.CreatedBy), UserId.FromGuid(row.CreatedBy));

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Alerts.Alert aggregate): RuntimeHelpers.GetUninitializedObject bypasses ALL
        // field initializers, so these readonly list-backed fields start out null, not the empty
        // list their inline initializers declare. GetListField only reads the current field
        // value (it does not create one), so Add() below -- or any public collection getter --
        // would NullReferenceException without this.
        AggregateReflectionRehydrator.SetField(widget, "_parameters", new List<WidgetParameter>());
        AggregateReflectionRehydrator.SetField(widget, "_permittedRoles", new List<string>());

        var paramRows = await queryExecutor.QueryAsync<WidgetParameterRow>(
            "[Dashboards].[usp_Widget_GetParameters]", new StoredProcedureBuilder().AddGuid("WidgetId", row.WidgetId).Build(), cancellationToken);
        var paramList = AggregateReflectionRehydrator.GetListField<DomainWidget, WidgetParameter>(widget, "_parameters");
        foreach (var p in paramRows)
            paramList.Add(WidgetParameter.Create(p.ParamKey, p.ParamValue));

        var roleRows = await queryExecutor.QueryAsync<WidgetPermittedRoleRow>(
            "[Dashboards].[usp_Widget_GetPermittedRoles]", new StoredProcedureBuilder().AddGuid("WidgetId", row.WidgetId).Build(), cancellationToken);
        var roleList = AggregateReflectionRehydrator.GetListField<DomainWidget, string>(widget, "_permittedRoles");
        foreach (var r in roleRows)
            roleList.Add(r.RoleName);

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Alerts.Alert aggregate, which shares this same AggregateReflectionRehydrator
        // helper): _domainEvents was never initialized here, so any lifecycle-transition method
        // calling AddDomainEvent() on a rehydrated aggregate (e.g. Activate()/Deprecate()) would
        // throw NullReferenceException.
        AggregateReflectionRehydrator.SetField(widget, "_domainEvents", new List<PlantSafety.Domain.Dashboards.Widget.Events.IWidgetDomainEvent>());

        return widget;
    }

    private async Task<WidgetDto> ToDtoAsync(WidgetRow row, CancellationToken cancellationToken)
    {
        var paramRows = await queryExecutor.QueryAsync<WidgetParameterRow>(
            "[Dashboards].[usp_Widget_GetParameters]", new StoredProcedureBuilder().AddGuid("WidgetId", row.WidgetId).Build(), cancellationToken);
        var roleRows = await queryExecutor.QueryAsync<WidgetPermittedRoleRow>(
            "[Dashboards].[usp_Widget_GetPermittedRoles]", new StoredProcedureBuilder().AddGuid("WidgetId", row.WidgetId).Build(), cancellationToken);

        return new WidgetDto
        {
            WidgetId = row.WidgetId,
            TenantId = row.TenantId,
            Status = row.Status,
            Name = row.Name,
            TypeCode = row.TypeCode,
            SourceModule = row.DataSourceModule,
            QueryOrEndpoint = row.DataSourceQueryOrEndpoint,
            RefreshIntervalSeconds = row.RefreshIntervalSeconds,
            Parameters = paramRows.ToDictionary(p => p.ParamKey, p => p.ParamValue),
            PermittedRoles = roleRows.Select(r => r.RoleName).ToList(),
        };
    }

    // ---- Dapper row-mapping POCOs ---------------------------------------------

    private sealed class WidgetRow
    {
        public Guid WidgetId { get; set; }
        public Guid TenantId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TypeCode { get; set; } = string.Empty;
        public string DataSourceModule { get; set; } = string.Empty;
        public string DataSourceQueryOrEndpoint { get; set; } = string.Empty;
        public int RefreshIntervalSeconds { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
    }

    private sealed class WidgetParameterRow
    {
        public Guid WidgetId { get; set; }
        public string ParamKey { get; set; } = string.Empty;
        public string ParamValue { get; set; } = string.Empty;
    }

    private sealed class WidgetPermittedRoleRow
    {
        public Guid WidgetId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
