// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a new Safety Budget allocation.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Financials.Commands.Budgets.CreateBudget;

/// <summary>
/// Creates a new Safety Budget record in Draft status.
/// Caller must hold <c>Financials.Budgets.Create</c>.
/// </summary>
public sealed record CreateBudgetCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site this budget is for. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year (e.g., 2026).</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Budget category.</summary>
    public required string Category { get; init; }

    /// <summary>Allocated amount.</summary>
    public required decimal AllocatedAmount { get; init; }

    /// <summary>Currency code (ISO 4217, e.g., USD).</summary>
    public required string CurrencyCode { get; init; }
}
