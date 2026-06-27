// ============================================================================
// File: PredictiveAnalyticsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PredictiveAnalytics endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PredictiveAnalyticsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PredictiveAnalytics-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PredictiveAnalytics-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.PredictiveAnalytics.Filters;

/// <summary>
/// Optional action filter for PredictiveAnalytics. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PredictiveAnalyticsActionFilter : IActionFilter
{
    private readonly ILogger<PredictiveAnalyticsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PredictiveAnalyticsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PredictiveAnalyticsActionFilter(ILogger<PredictiveAnalyticsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PredictiveAnalytics action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}