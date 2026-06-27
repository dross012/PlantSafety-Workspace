// ============================================================================
// File: EngagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Optional per-sub-area action filter for Engagement Analytics endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EngagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed beyond the global ApiValidationFilter.
//
// Rationale:
// - Allows Engagement-specific request enrichment (injecting default date ranges, etc.) without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting to inject default From/To date values when not supplied by the caller.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Filters;

/// <summary>
/// Optional action filter for Engagement Analytics. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EngagementActionFilter : IActionFilter
{
    private readonly ILogger<EngagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EngagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EngagementActionFilter(ILogger<EngagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Engagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}