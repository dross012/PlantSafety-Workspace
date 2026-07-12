// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single risk forecast.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Predictive.RiskForecasting.Models;

/// <summary>A single forward-looking risk forecast data point.</summary>
public sealed record RiskForecastingItemDto
{
    /// <summary>Forecast identifier.</summary>
    public required Guid ForecastId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Risk category being forecast (e.g., Overall Site Risk, Chemical, Ergonomic).</summary>
    public required string RiskCategory { get; init; }

    /// <summary>Forecast risk value for the horizon.</summary>
    public required decimal ForecastValue { get; init; }

    /// <summary>Number of days into the future this forecast covers.</summary>
    public required int ForecastHorizonDays { get; init; }

    /// <summary>Current status of the forecast (e.g., Active, Superseded).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the forecast was generated.</summary>
    public required DateTime ForecastedAtUtc { get; init; }
}
