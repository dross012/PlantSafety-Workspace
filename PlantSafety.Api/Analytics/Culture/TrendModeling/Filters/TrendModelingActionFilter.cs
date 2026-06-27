// ============================================================================
// File: TrendModelingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for TrendModeling endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(TrendModelingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows TrendModeling-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for TrendModeling-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Culture.TrendModeling.Filters;

/// <summary>
/// Optional action filter for TrendModeling. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class TrendModelingActionFilter : IActionFilter
{
    private readonly ILogger<TrendModelingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="TrendModelingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public TrendModelingActionFilter(ILogger<TrendModelingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("TrendModeling action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}