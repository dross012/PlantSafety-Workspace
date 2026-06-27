// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Budget record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Models.Budgets;

/// <summary>
/// Read-side projection of a SafetyBudget aggregate.
/// Safety budgets track planned vs. actual safety expenditure per fiscal period.
/// </summary>
public sealed record SafetyBudgetDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid BudgetId { get; init; }

    /// <summary>Tenant owning this budget.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this budget is allocated to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year this budget covers (e.g., 2026).</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Budget category (e.g., PPE, Training, Equipment, Compliance, Investigations).</summary>
    public required string Category { get; init; }

    /// <summary>Planned/allocated budget amount.</summary>
    public required decimal AllocatedAmount { get; init; }

    /// <summary>Currency code (ISO 4217).</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>Total actual spend to date against this budget.</summary>
    public required decimal ActualSpend { get; init; }

    /// <summary>Remaining balance (AllocatedAmount - ActualSpend).</summary>
    public required decimal RemainingBalance { get; init; }

    /// <summary>Workflow status (e.g., Draft, Active, Approved, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
