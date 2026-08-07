// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Infrastructure
//  File:           HazmatMaterialDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IHazmatMaterialRepository (Domain repository
//                  interface for the HazmatMaterial aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because HazmatMaterial exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write -- a
//                  platform-wide bug fixed elsewhere in this program was exactly UpdateAsync
//                  forgetting to do this, so it is deliberately present here from day one.
//                  GetByIdAsync/GetByUnNumberAsync are tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag from day one. SpecialProvisions is list-valued
//                  on the aggregate and is persisted as a JSON array string (see
//                  PHMSA_HazmatMaterial.sql). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Events;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Classification.Persistence;

/// <summary>Dapper-backed repository for the <see cref="HazmatMaterial"/> aggregate root.</summary>
public sealed class HazmatMaterialDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazmatMaterialDapperRepository> logger)
    : IHazmatMaterialRepository
{
    /// <inheritdoc/>
    public async Task<HazmatMaterial?> GetByIdAsync(HazmatMaterialId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazmatMaterial {HazmatMaterialId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatMaterialId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatMaterialRow>(
            "[Compliance].[usp_PHMSAHazmatMaterial_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<HazmatMaterial?> GetByUnNumberAsync(UNNumber unNumber, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetByUnNumber for HazmatMaterial {UNNumber} Tenant {TenantId}", unNumber.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddString("UNNumber", unNumber.Value, 20)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatMaterialRow>(
            "[Compliance].[usp_PHMSAHazmatMaterial_GetByUNNumber]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(HazmatMaterial aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add HazmatMaterial {HazmatMaterialId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatMaterialId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("UNNumber", aggregate.UNNumber.Value, 20)
            .AddString("ProperShippingName", aggregate.ProperShippingName.Value, 300)
            .AddString("HazardClass", aggregate.HazardClass.ToString(), 10)
            .AddString("PackingGroup", aggregate.PackingGroup.ToString(), 20)
            .AddString("SpecialProvisions", JsonSerializer.Serialize(aggregate.SpecialProvisions), size: -1)
            .AddString("ERGGuideNumber", aggregate.ERGGuideNumber, 10)
            .AddBool("IsMarinePollutant", aggregate.IsMarinePollutant)
            .AddBool("IsTIH", aggregate.IsTIH)
            .AddString("TIHZone", aggregate.TIHZone?.ToString(), 5)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_PHMSAHazmatMaterial_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised by
        // the aggregate during this write so TransactionalUnitOfWork.CommitAsync (invoked by
        // TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(HazmatMaterial aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update HazmatMaterial {HazmatMaterialId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatMaterialId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("UNNumber", aggregate.UNNumber.Value, 20)
            .AddString("ProperShippingName", aggregate.ProperShippingName.Value, 300)
            .AddString("HazardClass", aggregate.HazardClass.ToString(), 10)
            .AddString("PackingGroup", aggregate.PackingGroup.ToString(), 20)
            .AddString("SpecialProvisions", JsonSerializer.Serialize(aggregate.SpecialProvisions), size: -1)
            .AddString("ERGGuideNumber", aggregate.ERGGuideNumber, 10)
            .AddBool("IsMarinePollutant", aggregate.IsMarinePollutant)
            .AddBool("IsTIH", aggregate.IsTIH)
            .AddString("TIHZone", aggregate.TIHZone?.ToString(), 5)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PHMSAHazmatMaterial_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_PHMSAHazmatMaterial_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"HazmatMaterial {aggregate.Id.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Supersede, Retire) would silently drop its
        // domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="HazmatMaterial"/> aggregate from its flat read-model row.</summary>
    private static HazmatMaterial Hydrate(HazmatMaterialRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<HazmatMaterial>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.Id), HazmatMaterialId.FromGuid(row.HazmatMaterialId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.Status), Enum.Parse<HazmatMaterialStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.UNNumber), new UNNumber(row.UNNumber));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.ProperShippingName), new ProperShippingName(row.ProperShippingName));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.HazardClass), ParseHazardClass(row.HazardClass));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.PackingGroup), Enum.Parse<PackingGroup>(row.PackingGroup));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.SpecialProvisions), (IReadOnlyList<string>)(JsonSerializer.Deserialize<List<string>>(row.SpecialProvisions) ?? []));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.ERGGuideNumber), row.ERGGuideNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.IsMarinePollutant), row.IsMarinePollutant);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.IsTIH), row.IsTIH);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.TIHZone), string.IsNullOrEmpty(row.TIHZone) ? (TIHZone?)null : Enum.Parse<TIHZone>(row.TIHZone));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatMaterial.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IHazmatMaterialDomainEvent>());

        return agg;
    }

    /// <summary>Parses a HazardClass stored value (e.g. "3" or "6.1") back into the value object.</summary>
    private static HazardClass ParseHazardClass(string value)
    {
        var parts = value.Split('.', 2);
        var hazardClass = int.Parse(parts[0]);
        var division = parts.Length > 1 ? parts[1] : null;
        return new HazardClass(hazardClass, division);
    }

    /// <summary>Flat read-model row shape returned by the HazmatMaterial stored procedures.</summary>
    private sealed record HazmatMaterialRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string). Not
        /// consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer HazmatMaterialDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid HazmatMaterialId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string UNNumber { get; init; }
        public required string ProperShippingName { get; init; }
        public required string HazardClass { get; init; }
        public required string PackingGroup { get; init; }
        public required string SpecialProvisions { get; init; }
        public string? ERGGuideNumber { get; init; }
        public bool IsMarinePollutant { get; init; }
        public bool IsTIH { get; init; }
        public string? TIHZone { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
