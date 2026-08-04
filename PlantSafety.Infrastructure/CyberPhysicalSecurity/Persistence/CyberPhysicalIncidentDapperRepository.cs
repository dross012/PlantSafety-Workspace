// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           CyberPhysicalIncidentDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the CyberPhysicalIncident aggregate root.
//                  Implements both the Domain write-side repository and the Application read-side
//                  repository. RelatedSabotageIndicatorIds is persisted as a delimited string
//                  column for simplicity (small, bounded collection; no independent lifecycle).
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Specifications;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

/// <summary>Dapper-backed repository for the <see cref="CyberPhysicalIncident"/> aggregate root.</summary>
public sealed class CyberPhysicalIncidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CyberPhysicalIncidentDapperRepository> logger)
    : ICyberPhysicalIncidentRepository, ICyberPhysicalIncidentReadRepository
{
    private const char RelatedIdsDelimiter = ';';

    /// <inheritdoc/>
    public async Task<CyberPhysicalIncident?> GetByIdAsync(CyberPhysicalIncidentId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("CyberPhysicalIncidentId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<CyberPhysicalIncidentRow>(
            "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    async Task<CyberPhysicalIncidentDto?> ICyberPhysicalIncidentReadRepository.GetByIdAsync(Guid cyberPhysicalIncidentId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("CyberPhysicalIncidentId", cyberPhysicalIncidentId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<CyberPhysicalIncidentRow>(
            "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CyberPhysicalIncident>> ListAsync(Guid tenantId, CyberPhysicalIncidentSpecification specification, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<CyberPhysicalIncidentRow>("[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetAll]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(CyberPhysicalIncident aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add CyberPhysicalIncident {CyberPhysicalIncidentId}", aggregate.CyberPhysicalIncidentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CyberPhysicalIncidentId", aggregate.CyberPhysicalIncidentId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Title", aggregate.Title, 256)
            .AddString("Description", aggregate.Description, 4000)
            .AddString("Severity", aggregate.Severity.ToString(), 20)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("RelatedSabotageIndicatorIds", string.Join(RelatedIdsDelimiter, aggregate.RelatedSabotageIndicatorIds), 4000)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("OpenedAtUtc", aggregate.OpenedAtUtc.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddInt("ResponseTimeMinutes", aggregate.ResponseTimeMinutes)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(CyberPhysicalIncident aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update CyberPhysicalIncident {CyberPhysicalIncidentId}", aggregate.CyberPhysicalIncidentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CyberPhysicalIncidentId", aggregate.CyberPhysicalIncidentId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddInt("ResponseTimeMinutes", aggregate.ResponseTimeMinutes)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    private void FlushDomainEvents(CyberPhysicalIncident aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());

        aggregate.ClearDomainEvents();
    }

    private static CyberPhysicalIncident Hydrate(CyberPhysicalIncidentRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CyberPhysicalIncident>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.CyberPhysicalIncidentId), CyberPhysicalIncidentId.FromGuid(row.CyberPhysicalIncidentId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.Title), row.Title);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.Severity), Enum.Parse<CyberPhysicalIncidentSeverity>(row.Severity));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.Status), Enum.Parse<CyberPhysicalIncidentStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.OpenedAtUtc), UtcTimestamp.From(row.OpenedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.ResponseTimeMinutes), row.ResponseTimeMinutes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CyberPhysicalIncident.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICyberPhysicalIncidentDomainEvent>());

        var relatedIds = ParseRelatedIds(row.RelatedSabotageIndicatorIds);
        var relatedIdsList = AggregateReflectionHydrator.GetPrivateCollectionField<CyberPhysicalIncident, List<Guid>>(agg, "_relatedSabotageIndicatorIds");
        relatedIdsList.AddRange(relatedIds);

        return agg;
    }

    private static CyberPhysicalIncidentDto ToDto(CyberPhysicalIncidentRow row) => new(
        row.CyberPhysicalIncidentId, row.TenantId, row.SiteId, row.Title, row.Description, row.Severity,
        row.Status, ParseRelatedIds(row.RelatedSabotageIndicatorIds), row.OwnerId, row.OpenedAtUtc,
        row.ClosedAtUtc, row.ResponseTimeMinutes);

    private static IReadOnlyList<Guid> ParseRelatedIds(string? raw)
        => string.IsNullOrWhiteSpace(raw)
            ? Array.Empty<Guid>()
            : raw.Split(RelatedIdsDelimiter, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();

    /// <summary>Flat read-model row shape returned by the CyberPhysicalIncident stored procedures.</summary>
    private sealed record CyberPhysicalIncidentRow
    {
        public required Guid CyberPhysicalIncidentId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Severity { get; init; }
        public required string Status { get; init; }
        public string? RelatedSabotageIndicatorIds { get; init; }
        public required Guid OwnerId { get; init; }
        public required DateTime OpenedAtUtc { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public int? ResponseTimeMinutes { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
