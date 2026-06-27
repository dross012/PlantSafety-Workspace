// ============================================================================
// File: RiskForecastingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for RiskForecasting endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RiskForecastingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows RiskForecasting-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for RiskForecasting-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Predictive.RiskForecasting.Filters;

/// <summary>
/// Optional action filter for RiskForecasting. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RiskForecastingActionFilter : IActionFilter
{
    private readonly ILogger<RiskForecastingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RiskForecastingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RiskForecastingActionFilter(ILogger<RiskForecastingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("RiskForecasting action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}