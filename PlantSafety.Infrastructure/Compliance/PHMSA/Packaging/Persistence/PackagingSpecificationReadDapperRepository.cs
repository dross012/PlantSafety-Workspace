// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Infrastructure
//  File:           PackagingSpecificationReadDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PackagingSpecification Application-layer read-model.
//                  Backs GetPackagingSpecQuery and ListPackagingSpecsQuery. Reuses
//                  usp_PHMSAPackagingSpecification_GetById (the same proc
//                  PackagingSpecificationDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate) and usp_PHMSAPackagingSpecification_GetAllActive rather
//                  than introducing duplicate stored procedures.
//                  usp_PHMSAPackagingSpecification_GetAllActive is not tenant-scoped in SQL
//                  (mirrors PermitsDapperRepository.ListAsync's identical precedent), so
//                  tenant/site filtering for ListAsync is applied in-memory over the returned
//                  candidate set. Queries project into a private Row shape (rather than directly
//                  into PackagingSpecificationDto) because PerformanceTests is a JSON array
//                  string column that Dapper cannot map directly onto the DTO's
//                  IReadOnlyList&lt;string&gt; property -- ToDto() below deserializes it. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Packaging.Models;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Repositories;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Packaging.Persistence;

/// <summary>Dapper-backed repository for PackagingSpecification read-model queries.</summary>
public sealed class PackagingSpecificationReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PackagingSpecificationReadDapperRepository> logger)
    : IPackagingSpecificationRepository
{
    /// <inheritdoc/>
    public async Task<PackagingSpecificationDto?> GetByIdAsync(Guid packagingSpecificationId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for PackagingSpecification {PackagingSpecificationId} Tenant {TenantId}", packagingSpecificationId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PackagingSpecificationId", packagingSpecificationId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PackagingSpecificationRow>(
            "[Compliance].[usp_PHMSAPackagingSpecification_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PackagingSpecificationDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? dotSpec,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List PackagingSpecification read-model for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddString("DOTSpec", dotSpec, 20)
            .Build();

        var rows = await queryExecutor.QueryAsync<PackagingSpecificationRow>(
            "[Compliance].[usp_PHMSAPackagingSpecification_GetAllActive]", parameters, cancellationToken);

        return rows
            .Where(r => r.TenantId == tenantId && (siteId is null || r.SiteId == siteId))
            .Select(ToDto)
            .ToList();
    }

    private static PackagingSpecificationDto ToDto(PackagingSpecificationRow row) => new()
    {
        PackagingSpecificationId = row.PackagingSpecificationId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        Status = row.Status,
        DOTSpec = row.DOTSpec,
        PerformanceTests = JsonSerializer.Deserialize<List<string>>(row.PerformanceTests) ?? [],
        MaxCapacity = row.MaxCapacity,
        CapacityUnit = row.CapacityUnit,
        ClosureInstructions = row.ClosureInstructions,
        ReuseRestrictions = row.ReuseRestrictions,
        AssociatedMaterialId = row.AssociatedMaterialId,
        CreatedAtUtc = row.CreatedAtUtc,
        CreatedBy = row.CreatedBy,
        ETag = row.ETag,
    };

    /// <summary>Flat read-model row shape returned by the PackagingSpecification stored procedures.</summary>
    private sealed record PackagingSpecificationRow
    {
        public string? ETag { get; init; }
        public required Guid PackagingSpecificationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DOTSpec { get; init; }
        public required string PerformanceTests { get; init; }
        public required decimal MaxCapacity { get; init; }
        public required string CapacityUnit { get; init; }
        public required string ClosureInstructions { get; init; }
        public string? ReuseRestrictions { get; init; }
        public Guid? AssociatedMaterialId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
