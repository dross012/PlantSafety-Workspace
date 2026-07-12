// ============================================================================
// File: AnalyticsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Analytics endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AnalyticsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Analytics-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Analytics-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analytics.Filters;

/// <summary>
/// Optional action filter for Integrations.Analytics. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AnalyticsActionFilter : IActionFilter
{
    private readonly ILogger<AnalyticsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AnalyticsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AnalyticsActionFilter(ILogger<AnalyticsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Analytics action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
