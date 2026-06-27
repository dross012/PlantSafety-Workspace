// ============================================================================
// File: TrendAnalysisActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for TrendAnalysis endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(TrendAnalysisActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows TrendAnalysis-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for TrendAnalysis-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.TrendAnalysis.Filters;

/// <summary>
/// Optional action filter for TrendAnalysis. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class TrendAnalysisActionFilter : IActionFilter
{
    private readonly ILogger<TrendAnalysisActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="TrendAnalysisActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public TrendAnalysisActionFilter(ILogger<TrendAnalysisActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("TrendAnalysis action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}