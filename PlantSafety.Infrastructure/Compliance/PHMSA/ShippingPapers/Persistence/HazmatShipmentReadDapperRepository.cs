// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Infrastructure
//  File:           HazmatShipmentReadDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazmatShipment Application-layer read-model. Backs
//                  GetHazmatShipmentQuery and ListHazmatShipmentsQuery. Reuses
//                  usp_PHMSAHazmatShipment_GetById (the same proc
//                  HazmatShipmentDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) plus usp_PHMSAHazmatShipment_GetAllActive (which accepts optional
//                  @Status/@Mode filters) for listing. SiteId filtering (not pushed down by the
//                  listing proc) is applied in-memory over the candidate set.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Repositories;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.ShippingPapers.Persistence;

/// <summary>Dapper-backed repository for HazmatShipment read-model queries.</summary>
public sealed class HazmatShipmentReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HazmatShipmentReadDapperRepository> logger)
    : IHazmatShipmentRepository
{
    /// <inheritdoc/>
    public async Task<HazmatShipmentDto?> GetByIdAsync(Guid hazmatShipmentId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for HazmatShipment {HazmatShipmentId} Tenant {TenantId}", hazmatShipmentId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatShipmentId", hazmatShipmentId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatShipmentRow>(
            "[Compliance].[usp_PHMSAHazmatShipment_GetById]", parameters, cancellationToken);

        return row is null ? null : Map(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazmatShipmentDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? mode,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List HazmatShipments for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddString("Status", status, 20)
            .AddString("Mode", mode, 10)
            .Build();

        var rows = await queryExecutor.QueryAsync<HazmatShipmentRow>(
            "[Compliance].[usp_PHMSAHazmatShipment_GetAllActive]", parameters, cancellationToken);

        return rows
            .Where(r => r.TenantId == tenantId && (!siteId.HasValue || r.SiteId == siteId))
            .Select(Map)
            .ToList();
    }

    private static HazmatShipmentDto Map(HazmatShipmentRow row) => new()
    {
        HazmatShipmentId = row.HazmatShipmentId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        Status = row.Status,
        MaterialIds = JsonSerializer.Deserialize<List<Guid>>(row.MaterialIds) ?? [],
        PackagingIds = JsonSerializer.Deserialize<List<Guid>>(row.PackagingIds) ?? [],
        Mode = row.Mode,
        ShippingPaperBasicDescription = row.ShippingPaperBasicDescription,
        ShippingPaperEmergencyResponseRef = row.ShippingPaperEmergencyResponseRef,
        ShippingPaperCertification = row.ShippingPaperCertification,
        ShippingPaperWasteManifestReference = row.ShippingPaperWasteManifestReference,
        ShippingPaperAdditionalModalRequirements = string.IsNullOrEmpty(row.ShippingPaperAdditionalModalRequirements)
            ? []
            : JsonSerializer.Deserialize<List<string>>(row.ShippingPaperAdditionalModalRequirements) ?? [],
        ShippingPaperGeneratedAtUtc = row.ShippingPaperGeneratedAtUtc,
        EmergencyResponseErgGuideNumber = row.EmergencyResponseErgGuideNumber,
        EmergencyResponseContactName = row.EmergencyResponseContactName,
        EmergencyResponseContactPhone = row.EmergencyResponseContactPhone,
        EmergencyResponseGuidanceSummary = row.EmergencyResponseGuidanceSummary,
        PlacardingRequirements = JsonSerializer.Deserialize<List<string>>(row.PlacardingRequirements) ?? [],
        SegregationViolations = (JsonSerializer.Deserialize<List<SegregationViolationRow>>(row.SegregationViolations) ?? [])
            .Select(v => new SegregationViolationDto(v.ClassA, v.ClassB, v.Reason, v.Severity))
            .ToList(),
        CreatedAtUtc = row.CreatedAtUtc,
        CreatedBy = row.CreatedBy,
        ETag = row.ETag,
    };

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
