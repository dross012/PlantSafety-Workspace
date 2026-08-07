// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Infrastructure
//  File:           HazmatContainerReadDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazmatContainer Application-layer read-model. Backs
//                  GetHazmatContainerQuery and ListHazmatContainersQuery. Reuses
//                  usp_PHMSAHazmatContainer_GetById (the same proc
//                  HazmatContainerDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) plus usp_PHMSAHazmatContainer_GetAllActive /
//                  usp_PHMSAHazmatContainer_GetByMaterialId for listing. SiteId filtering (not
//                  pushed down by either listing proc) is applied in-memory over the candidate
//                  set, mirroring PermitsReadDapperRepository's precedent for filters the
//                  underlying proc does not accept as a parameter.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Placarding.Models;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Repositories;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Placarding.Persistence;

/// <summary>Dapper-backed repository for HazmatContainer read-model queries.</summary>
public sealed class HazmatContainerReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HazmatContainerReadDapperRepository> logger)
    : IHazmatContainerRepository
{
    /// <inheritdoc/>
    public async Task<HazmatContainerDto?> GetByIdAsync(Guid hazmatContainerId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for HazmatContainer {HazmatContainerId} Tenant {TenantId}", hazmatContainerId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatContainerId", hazmatContainerId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatContainerRow>(
            "[Compliance].[usp_PHMSAHazmatContainer_GetById]", parameters, cancellationToken);

        return row is null ? null : Map(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazmatContainerDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        Guid? materialId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List HazmatContainers for Tenant {TenantId}", tenantId);

        IReadOnlyList<HazmatContainerRow> rows;
        if (materialId.HasValue)
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("MaterialId", materialId.Value)
                .AddGuid("TenantId", tenantId)
                .Build();

            rows = await queryExecutor.QueryAsync<HazmatContainerRow>(
                "[Compliance].[usp_PHMSAHazmatContainer_GetByMaterialId]", parameters, cancellationToken);
        }
        else
        {
            rows = await queryExecutor.QueryAsync<HazmatContainerRow>(
                "[Compliance].[usp_PHMSAHazmatContainer_GetAllActive]", new DynamicParameters(), cancellationToken);
        }

        return rows
            .Where(r => r.TenantId == tenantId && (!siteId.HasValue || r.SiteId == siteId))
            .Select(Map)
            .ToList();
    }

    private static HazmatContainerDto Map(HazmatContainerRow row) => new()
    {
        HazmatContainerId = row.HazmatContainerId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        Status = row.Status,
        ContainerType = row.ContainerType,
        MaterialId = row.MaterialId,
        PackagingSpecId = row.PackagingSpecId,
        Markings = JsonSerializer.Deserialize<List<string>>(row.Markings) ?? [],
        Labels = JsonSerializer.Deserialize<List<string>>(row.Labels) ?? [],
        Placards = JsonSerializer.Deserialize<List<string>>(row.Placards) ?? [],
        OrientationArrowsRequired = row.OrientationArrowsRequired,
        CompatibilityGroup = row.CompatibilityGroup,
        CreatedAtUtc = row.CreatedAtUtc,
        CreatedBy = row.CreatedBy,
        ETag = row.ETag,
    };

    /// <summary>Flat read-model row shape returned by <c>usp_PHMSAHazmatContainer_GetById</c>/<c>_GetAllActive</c>/<c>_GetByMaterialId</c>.</summary>
    private sealed record HazmatContainerRow
    {
        public string? ETag { get; init; }
        public required Guid HazmatContainerId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ContainerType { get; init; }
        public required Guid MaterialId { get; init; }
        public required Guid PackagingSpecId { get; init; }
        public required string Markings { get; init; }
        public required string Labels { get; init; }
        public required string Placards { get; init; }
        public required bool OrientationArrowsRequired { get; init; }
        public string? CompatibilityGroup { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
