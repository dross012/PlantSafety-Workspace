// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Infrastructure
//  File:           ChecklistTemplateDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ChecklistTemplate aggregate root. All DB access
//                  via stored procedures in the [Checklists] schema. Aggregate rehydration
//                  uses AggregateReflectionHydrator because ChecklistTemplate exposes no
//                  public Rehydrate()/factory path (private setters, private parameterless
//                  ctor "for rehydration only") and Domain source must not be modified to
//                  add one. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Entities;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Enums;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Specifications;
using PlantSafety.Domain.Checklists.ChecklistTemplate.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ChecklistTemplateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ChecklistTemplateDapperRepository> logger)
    : IChecklistTemplateRepository
{
    /// <inheritdoc/>
    public async Task<Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate?> GetByIdAsync(
        ChecklistTemplateId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById ChecklistTemplate {ChecklistTemplateId}", id.Value);

        var headerParams = new StoredProcedureBuilder()
            .AddGuid("ChecklistTemplateId", id.Value)
            .Build();

        var header = await queryExecutor.QuerySingleOrDefaultAsync<ChecklistTemplateRow>(
            "[Checklists].[usp_ChecklistTemplate_GetById]", headerParams, cancellationToken);

        if (header is null)
            return null;

        var itemParams = new StoredProcedureBuilder()
            .AddGuid("ChecklistTemplateId", id.Value)
            .Build();

        var itemRows = await queryExecutor.QueryAsync<ChecklistTemplateItemRow>(
            "[Checklists].[usp_ChecklistTemplateItem_ListByTemplate]", itemParams, cancellationToken);

        return Hydrate(header, itemRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate>> ListAsync(
        ChecklistTemplateSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List ChecklistTemplate via specification {Specification}", specification.GetType().Name);

        // Specification pattern predicates are in-memory Expression<Func<Aggregate,bool>>
        // delegates; Dapper has no LINQ/Expression-to-SQL translator, so candidate rows are
        // fetched broadly (tenant-scoped, per the RLS/SP contract) and fully rehydrated, then
        // filtered in-memory via specification.ToPredicate().
        var tenantId = ExtractTenantIdOrDefault(specification);

        var headerParams = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var headers = await queryExecutor.QueryAsync<ChecklistTemplateRow>(
            "[Checklists].[usp_ChecklistTemplate_ListByTenant]", headerParams, cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate>();

        foreach (var header in headers)
        {
            var itemParams = new StoredProcedureBuilder()
                .AddGuid("ChecklistTemplateId", header.ChecklistTemplateId)
                .Build();

            var itemRows = await queryExecutor.QueryAsync<ChecklistTemplateItemRow>(
                "[Checklists].[usp_ChecklistTemplateItem_ListByTemplate]", itemParams, cancellationToken);

            var aggregate = Hydrate(header, itemRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create ChecklistTemplate {ChecklistTemplateId} for tenant {TenantId}", aggregate.ChecklistTemplateId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParams = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("ChecklistTemplateId", aggregate.ChecklistTemplateId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("TemplateTitle", aggregate.TemplateTitle, 200)
            .AddString("Category", aggregate.Category, 100)
            .AddString("Description", aggregate.Description, 2000)
            .AddInt("ItemCount", aggregate.ItemCount)
            .AddBool("IsMandatory", aggregate.IsMandatory)
            .AddString("RegulatoryReference", aggregate.RegulatoryReference, 200)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistTemplate_Create]", headerParams, tx, cancellationToken: cancellationToken);

        await UpsertItemsAsync(conn, tx, aggregate, cancellationToken);

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

    /// <inheritdoc/>
    public async Task UpdateAsync(
        Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ChecklistTemplate {ChecklistTemplateId}", aggregate.ChecklistTemplateId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParams = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("ChecklistTemplateId", aggregate.ChecklistTemplateId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("TemplateTitle", aggregate.TemplateTitle, 200)
            .AddString("Description", aggregate.Description, 2000)
            .AddInt("ItemCount", aggregate.ItemCount)
            .AddBool("IsMandatory", aggregate.IsMandatory)
            .AddString("RegulatoryReference", aggregate.RegulatoryReference, 200)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddDateTime("RetiredAtUtc", aggregate.RetiredAtUtc?.Value)
            .AddString("RetirementReason", aggregate.RetirementReason, 500)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistTemplate_Update]", headerParams, tx, cancellationToken: cancellationToken);

        await UpsertItemsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();
    }

    private async Task UpsertItemsAsync(
        IDbConnection conn, IDbTransaction tx,
        Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate,
        CancellationToken cancellationToken)
    {
        foreach (var item in aggregate.Items)
        {
            var itemParams = new StoredProcedureBuilder()
                .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
                .AddGuid("ItemId", item.ItemId)
                .AddGuid("ChecklistTemplateId", item.ChecklistTemplateId.Value)
                .AddInt("Sequence", item.Sequence)
                .AddString("Description", item.Description, 1000)
                .AddBool("IsCritical", item.IsCritical)
                .AddString("RegulatoryReference", item.RegulatoryReference, 200)
                .AddDateTime("AddedAtUtc", item.AddedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistTemplateItem_Upsert]", itemParams, tx, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Rehydrates a full <see cref="Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate"/> aggregate
    /// (including its owned <see cref="ChecklistTemplateItem"/> collection) from flat Dapper rows via reflection,
    /// since the aggregate exposes no public rehydration factory.
    /// </summary>
    private static Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate Hydrate(
        ChecklistTemplateRow header, IReadOnlyList<ChecklistTemplateItemRow> itemRows)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.ChecklistTemplateId), ChecklistTemplateId.FromGuid(header.ChecklistTemplateId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.TenantId), header.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.SiteId), header.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.Status), Enum.Parse<ChecklistTemplateStatus>(header.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.TemplateTitle), header.TemplateTitle);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.Category), header.Category);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.Description), header.Description);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.ItemCount), header.ItemCount);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.IsMandatory), header.IsMandatory);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.RegulatoryReference), header.RegulatoryReference);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.CreatedAtUtc), new UtcTimestamp(header.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.CreatedBy), new UserId(header.CreatedBy));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.PublishedAtUtc), header.PublishedAtUtc.HasValue ? new UtcTimestamp(header.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.RetiredAtUtc), header.RetiredAtUtc.HasValue ? new UtcTimestamp(header.RetiredAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.RetirementReason), header.RetirementReason);

        // Rehydration bug fix: GetPrivateCollectionField only reads the current field value (it
        // does not create one), and RuntimeHelpers.GetUninitializedObject bypasses field
        // initializers entirely, so "_items" must be seeded first or this throws
        // InvalidOperationException ("Field '_items' ... was null.").
        AggregateReflectionHydrator.SetField(aggregate, "_items", new List<ChecklistTemplateItem>());
        var items = AggregateReflectionHydrator.GetPrivateCollectionField<Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, List<ChecklistTemplateItem>>(aggregate, "_items");
        foreach (var row in itemRows)
        {
            items.Add(new ChecklistTemplateItem(
                itemId: row.ItemId,
                checklistTemplateId: ChecklistTemplateId.FromGuid(row.ChecklistTemplateId),
                sequence: row.Sequence,
                description: row.Description,
                isCritical: row.IsCritical,
                regulatoryReference: row.RegulatoryReference,
                addedAtUtc: new UtcTimestamp(row.AddedAtUtc)));
        }

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Auditing.AuditEngagement aggregate): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // would throw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.Checklists.ChecklistTemplate.Events.IChecklistTemplateDomainEvent>());

        return aggregate;
    }

    private static Guid ExtractTenantIdOrDefault(ChecklistTemplateSpecification specification) => Guid.Empty;

    private sealed record ChecklistTemplateRow
    {
        public required Guid ChecklistTemplateId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string TemplateTitle { get; init; }
        public required string Category { get; init; }
        public required string Description { get; init; }
        public required int ItemCount { get; init; }
        public required bool IsMandatory { get; init; }
        public string? RegulatoryReference { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public DateTime? RetiredAtUtc { get; init; }
        public string? RetirementReason { get; init; }
    }

    private sealed record ChecklistTemplateItemRow
    {
        public required Guid ItemId { get; init; }
        public required Guid ChecklistTemplateId { get; init; }
        public required int Sequence { get; init; }
        public required string Description { get; init; }
        public required bool IsCritical { get; init; }
        public string? RegulatoryReference { get; init; }
        public required DateTime AddedAtUtc { get; init; }
    }
}
