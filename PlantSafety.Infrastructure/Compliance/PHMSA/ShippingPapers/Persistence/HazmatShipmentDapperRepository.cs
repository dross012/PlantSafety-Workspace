// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Infrastructure
//  File:           HazmatShipmentDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IHazmatShipmentRepository (Domain repository
//                  interface for the HazmatShipment aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema (usp_PHMSAHazmatShipment_*). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because HazmatShipment exposes
//                  only a private "for rehydration only" constructor and private-setter
//                  properties. MaterialIds/PackagingIds/PlacardingRequirements/
//                  SegregationViolations and the modal requirements list are stored as
//                  NVARCHAR(MAX) JSON and (de)serialized with System.Text.Json; the generated
//                  ShippingPaper and EmergencyResponseInfo owned value objects are flattened onto
//                  discrete columns rather than a child table. Both AddAsync AND UpdateAsync
//                  enqueue domain events to the outbox after their write (platform-wide fix,
//                  UpdateAsync must not skip this). GetByIdAsync is tenant-scoped and UpdateAsync
//                  enforces optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.ShippingPapers.Persistence;

/// <summary>Dapper-backed repository for the <see cref="HazmatShipment"/> aggregate root.</summary>
public sealed class HazmatShipmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazmatShipmentDapperRepository> logger)
    : IHazmatShipmentRepository
{
    /// <inheritdoc/>
    public async Task<HazmatShipment?> GetByIdAsync(HazmatShipmentId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazmatShipment {HazmatShipmentId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatShipmentId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatShipmentRow>(
            "[Compliance].[usp_PHMSAHazmatShipment_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(HazmatShipment aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add HazmatShipment {HazmatShipmentId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildWriteParameters(aggregate, isInsert: true)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_PHMSAHazmatShipment_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(HazmatShipment aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update HazmatShipment {HazmatShipmentId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildWriteParameters(aggregate, isInsert: false)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_PHMSAHazmatShipment_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_PHMSAHazmatShipment_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"HazmatShipment {aggregate.Id.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Builds the parameters shared by Insert and Update (all columns except audit/CreatedBy/ETag, which callers append).</summary>
    private static StoredProcedureBuilder BuildWriteParameters(HazmatShipment aggregate, bool isInsert)
    {
        var builder = new StoredProcedureBuilder()
            .AddGuid("HazmatShipmentId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId);

        if (isInsert)
        {
            builder.AddGuid("SiteId", aggregate.SiteId);
        }

        builder
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("MaterialIds", JsonSerializer.Serialize(aggregate.MaterialIds.Select(m => m.Value)), size: -1)
            .AddString("PackagingIds", JsonSerializer.Serialize(aggregate.PackagingIds.Select(p => p.Value)), size: -1)
            .AddString("ShippingPaperBasicDescription", aggregate.ShippingPaper?.BasicDescriptionSequence, size: -1)
            .AddString("ShippingPaperEmergencyResponseRef", aggregate.ShippingPaper?.EmergencyResponseInformationRef, size: -1)
            .AddString("ShippingPaperCertification", aggregate.ShippingPaper?.ShipperCertification, size: -1)
            .AddString("ShippingPaperWasteManifestReference", aggregate.ShippingPaper?.WasteManifestReference, size: -1)
            .AddString("ShippingPaperAdditionalModalRequirements", aggregate.ShippingPaper is null ? null : JsonSerializer.Serialize(aggregate.ShippingPaper.AdditionalModalRequirements), size: -1)
            .AddDateTime("ShippingPaperGeneratedAtUtc", aggregate.ShippingPaper?.GeneratedAtUtc.Value)
            .AddString("EmergencyResponseErgGuideNumber", aggregate.EmergencyResponseInfo?.ErgGuideNumber, 10)
            .AddString("EmergencyResponseContactName", aggregate.EmergencyResponseInfo?.EmergencyContactName, 200)
            .AddString("EmergencyResponseContactPhone", aggregate.EmergencyResponseInfo?.EmergencyContactPhone, 50)
            .AddString("EmergencyResponseGuidanceSummary", aggregate.EmergencyResponseInfo?.ResponseGuidanceSummary, size: -1)
            .AddString("Mode", aggregate.Mode.ToString(), 10)
            .AddString("PlacardingRequirements", JsonSerializer.Serialize(aggregate.PlacardingRequirements), size: -1)
            .AddString("SegregationViolations", JsonSerializer.Serialize(aggregate.SegregationViolations.Select(v => new SegregationViolationRow(v.ClassA.ToString(), v.ClassB.ToString(), v.Reason, v.Severity.ToString()))), size: -1);

        return builder;
    }

    /// <summary>Reconstructs a <see cref="HazmatShipment"/> aggregate from its flat read-model row.</summary>
    private static HazmatShipment Hydrate(HazmatShipmentRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<HazmatShipment>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.Id), HazmatShipmentId.FromGuid(row.HazmatShipmentId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.Status), Enum.Parse<HazmatShipmentStatus>(row.Status));

        var materialIds = (JsonSerializer.Deserialize<List<Guid>>(row.MaterialIds) ?? [])
            .Select(HazmatMaterialId.FromGuid).ToList();
        AggregateReflectionHydrator.SetField(agg, "_materialIds", materialIds);

        var packagingIds = (JsonSerializer.Deserialize<List<Guid>>(row.PackagingIds) ?? [])
            .Select(PackagingSpecificationId.FromGuid).ToList();
        AggregateReflectionHydrator.SetField(agg, "_packagingIds", packagingIds);

        if (!string.IsNullOrEmpty(row.ShippingPaperBasicDescription))
        {
            var modalRequirements = string.IsNullOrEmpty(row.ShippingPaperAdditionalModalRequirements)
                ? []
                : JsonSerializer.Deserialize<List<string>>(row.ShippingPaperAdditionalModalRequirements) ?? [];

            var shippingPaper = new ShippingPaper(
                row.ShippingPaperBasicDescription,
                row.ShippingPaperEmergencyResponseRef ?? string.Empty,
                row.ShippingPaperCertification ?? string.Empty,
                row.ShippingPaperWasteManifestReference,
                modalRequirements,
                row.ShippingPaperGeneratedAtUtc.HasValue ? UtcTimestamp.From(row.ShippingPaperGeneratedAtUtc.Value) : UtcTimestamp.Now());

            AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.ShippingPaper), shippingPaper);
        }

        if (!string.IsNullOrEmpty(row.EmergencyResponseErgGuideNumber))
        {
            var emergencyResponseInfo = new EmergencyResponseInfo(
                row.EmergencyResponseErgGuideNumber,
                row.EmergencyResponseContactName ?? string.Empty,
                row.EmergencyResponseContactPhone ?? string.Empty,
                row.EmergencyResponseGuidanceSummary ?? string.Empty);

            AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.EmergencyResponseInfo), emergencyResponseInfo);
        }

        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.Mode), Enum.Parse<TransportMode>(row.Mode));

        var placardingRequirements = JsonSerializer.Deserialize<List<string>>(row.PlacardingRequirements) ?? [];
        AggregateReflectionHydrator.SetField(agg, "_placardingRequirements", placardingRequirements);

        var violationRows = JsonSerializer.Deserialize<List<SegregationViolationRow>>(row.SegregationViolations) ?? [];
        var violations = violationRows
            .Select(v => new SegregationViolation(ParseHazardClass(v.ClassA), ParseHazardClass(v.ClassB), v.Reason, Enum.Parse<SegregationSeverity>(v.Severity)))
            .ToList();
        AggregateReflectionHydrator.SetField(agg, "_segregationViolations", violations);

        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatShipment.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IHazmatShipmentDomainEvent>());

        return agg;
    }

    private static HazardClass ParseHazardClass(string value)
    {
        var parts = value.Split('.', 2);
        return new HazardClass(int.Parse(parts[0]), parts.Length > 1 ? parts[1] : null);
    }

    /// <summary>Flattened JSON shape for a single segregation violation stored in the SegregationViolations column.</summary>
    private sealed record SegregationViolationRow(string ClassA, string ClassB, string Reason, string Severity);

    /// <summary>Flat read-model row shape returned by <c>usp_PHMSAHazmatShipment_GetById</c>/<c>_GetAllActive</c>.</summary>
    private sealed record HazmatShipmentRow
    {
        public string? ETag { get; init; }
        public required Guid HazmatShipmentId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string MaterialIds { get; init; }
        public required string PackagingIds { get; init; }
        public string? ShippingPaperBasicDescription { get; init; }
        public string? ShippingPaperEmergencyResponseRef { get; init; }
        public string? ShippingPaperCertification { get; init; }
        public string? ShippingPaperWasteManifestReference { get; init; }
        public string? ShippingPaperAdditionalModalRequirements { get; init; }
        public DateTime? ShippingPaperGeneratedAtUtc { get; init; }
        public string? EmergencyResponseErgGuideNumber { get; init; }
        public string? EmergencyResponseContactName { get; init; }
        public string? EmergencyResponseContactPhone { get; init; }
        public string? EmergencyResponseGuidanceSummary { get; init; }
        public required string Mode { get; init; }
        public required string PlacardingRequirements { get; init; }
        public required string SegregationViolations { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
