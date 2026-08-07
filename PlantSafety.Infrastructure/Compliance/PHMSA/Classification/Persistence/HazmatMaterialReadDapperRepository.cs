// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Infrastructure
//  File:           HazmatMaterialReadDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazmatMaterial Application-layer read-model. Backs
//                  GetHazmatMaterialQuery and ListClassifiedMaterialsQuery. Reuses
//                  usp_PHMSAHazmatMaterial_GetById (the same proc HazmatMaterialDapperRepository.
//                  GetByIdAsync uses to hydrate the write-side aggregate) and
//                  usp_PHMSAHazmatMaterial_GetAllActive rather than introducing duplicate stored
//                  procedures. usp_PHMSAHazmatMaterial_GetAllActive is not tenant-scoped in SQL
//                  (mirrors PermitsDapperRepository.ListAsync's identical precedent for the
//                  Domain interface carrying no tenant/site scoping parameter to push down to
//                  SQL), so tenant/site filtering for ListAsync is applied in-memory over the
//                  returned candidate set. Queries project into a private Row shape (rather than
//                  directly into HazmatMaterialDto) because SpecialProvisions is a JSON array
//                  string column that Dapper cannot map directly onto the DTO's
//                  IReadOnlyList&lt;string&gt; property -- ToDto() below deserializes it. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Classification.Persistence;

/// <summary>Dapper-backed repository for HazmatMaterial read-model queries.</summary>
public sealed class HazmatMaterialReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HazmatMaterialReadDapperRepository> logger)
    : IHazmatMaterialRepository
{
    /// <inheritdoc/>
    public async Task<HazmatMaterialDto?> GetByIdAsync(Guid hazmatMaterialId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for HazmatMaterial {HazmatMaterialId} Tenant {TenantId}", hazmatMaterialId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatMaterialId", hazmatMaterialId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatMaterialRow>(
            "[Compliance].[usp_PHMSAHazmatMaterial_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazmatMaterialDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? unNumber,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List HazmatMaterial read-model for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddString("UNNumber", unNumber, 20)
            .Build();

        var rows = await queryExecutor.QueryAsync<HazmatMaterialRow>(
            "[Compliance].[usp_PHMSAHazmatMaterial_GetAllActive]", parameters, cancellationToken);

        return rows
            .Where(r => r.TenantId == tenantId && (siteId is null || r.SiteId == siteId))
            .Select(ToDto)
            .ToList();
    }

    private static HazmatMaterialDto ToDto(HazmatMaterialRow row) => new()
    {
        HazmatMaterialId = row.HazmatMaterialId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        Status = row.Status,
        UNNumber = row.UNNumber,
        ProperShippingName = row.ProperShippingName,
        HazardClass = row.HazardClass,
        PackingGroup = row.PackingGroup,
        SpecialProvisions = JsonSerializer.Deserialize<List<string>>(row.SpecialProvisions) ?? [],
        ERGGuideNumber = row.ERGGuideNumber,
        IsMarinePollutant = row.IsMarinePollutant,
        IsTIH = row.IsTIH,
        TIHZone = row.TIHZone,
        CreatedAtUtc = row.CreatedAtUtc,
        CreatedBy = row.CreatedBy,
        ETag = row.ETag,
    };

    /// <summary>Flat read-model row shape returned by the HazmatMaterial stored procedures.</summary>
    private sealed record HazmatMaterialRow
    {
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
