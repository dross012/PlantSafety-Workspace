// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the SafetyROI read/write model.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Handlers.ROI;

/// <summary>Repository for SafetyROI read/write operations.</summary>
public interface IRoiRepository
{
    /// <summary>Creates a new SafetyROI calculation record; stages a <c>RoiCalculatedEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid roiCalculationId,
        Guid tenantId,
        Guid? siteId,
        int fiscalYear,
        decimal totalInvestment,
        decimal estimatedAvoidedCosts,
        decimal netBenefit,
        decimal roiPercentage,
        string currencyCode,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the most recent read model for the given tenant/site/fiscal year, or null if not found.</summary>
    Task<RoiReadModel?> GetByFiscalYearAsync(Guid tenantId, Guid? siteId, int fiscalYear, CancellationToken cancellationToken);

    /// <summary>Returns paged calculation history matching the given filters.</summary>
    Task<(IReadOnlyList<RoiReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for SafetyROI stored-procedure result sets.</summary>
public sealed record RoiReadModel
{
    /// <summary>ROI calculation identifier.</summary>
    public required Guid RoiCalculationId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Fiscal year.</summary>
    public required int FiscalYear { get; init; }
    /// <summary>Total safety investment.</summary>
    public required decimal TotalInvestment { get; init; }
    /// <summary>Estimated avoided costs.</summary>
    public required decimal EstimatedAvoidedCosts { get; init; }
    /// <summary>Net benefit.</summary>
    public required decimal NetBenefit { get; init; }
    /// <summary>ROI percentage.</summary>
    public required decimal RoiPercentage { get; init; }
    /// <summary>ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }
    /// <summary>UTC timestamp when calculated.</summary>
    public required DateTimeOffset CalculatedAtUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token, projected from the row's native RowVersion column
    /// (<c>CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)</c>). Not yet consumed by any
    /// write path -- IRoiRepository.CreateAsync has no ETag concept and takes no @ETag parameter --
    /// present so a future optimistic-concurrency pass has it available without another read-model
    /// change, matching the schema-only precedent set by RiskManagement's RiskRow.ETag /
    /// AccidentManagement's AccidentRow.ETag.
    /// </summary>
    public string? ETag { get; init; }
}
