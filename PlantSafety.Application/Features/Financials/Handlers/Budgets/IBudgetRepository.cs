// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the SafetyBudget aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Handlers.Budgets;

/// <summary>Repository for SafetyBudget read/write operations.</summary>
public interface IBudgetRepository
{
    /// <summary>Creates a new SafetyBudget; stages a <c>BudgetCreatedEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid budgetId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        decimal allocatedAmount,
        string currencyCode,
        int fiscalYear,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<BudgetReadModel?> GetByIdAsync(Guid tenantId, Guid budgetId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<BudgetReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        int? fiscalYear,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for SafetyBudget stored-procedure result sets.</summary>
public sealed record BudgetReadModel
{
    /// <summary>Budget identifier.</summary>
    public required Guid BudgetId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Allocated amount.</summary>
    public required decimal AllocatedAmount { get; init; }
    /// <summary>Actual spend to date.</summary>
    public required decimal ActualSpend { get; init; }
    /// <summary>Remaining balance.</summary>
    public required decimal RemainingBalance { get; init; }
    /// <summary>ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }
    /// <summary>Fiscal year.</summary>
    public required int FiscalYear { get; init; }
    /// <summary>Status: Draft/Active/Approved/Closed.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
