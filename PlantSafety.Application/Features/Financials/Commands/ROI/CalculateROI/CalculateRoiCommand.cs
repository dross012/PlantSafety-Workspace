// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to record a computed Safety ROI calculation for a fiscal period.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Financials.Commands.ROI.CalculateROI;

/// <summary>
/// Records a new Safety ROI calculation comparing total safety investment against
/// estimated avoided costs for a fiscal period. Caller must hold <c>Financials.ROI.Calculate</c>.
/// </summary>
public sealed record CalculateRoiCommand : ICommand<Guid>
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

    /// <summary>Site this calculation is for. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year of the calculation.</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Total safety investment (sum of budget spend) for the period.</summary>
    public required decimal TotalInvestment { get; init; }

    /// <summary>Estimated avoided costs (workers comp savings, incident cost avoidance, regulatory fines avoided).</summary>
    public required decimal EstimatedAvoidedCosts { get; init; }

    /// <summary>Currency code (ISO 4217, e.g., USD).</summary>
    public required string CurrencyCode { get; init; }
}
