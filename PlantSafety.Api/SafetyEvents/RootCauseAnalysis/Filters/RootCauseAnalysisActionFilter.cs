// ============================================================================
// File: RootCauseAnalysisActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for RootCauseAnalysis endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RootCauseAnalysisActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows RootCauseAnalysis-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for RootCauseAnalysis-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Filters;

/// <summary>
/// Optional action filter for RootCauseAnalysis. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RootCauseAnalysisActionFilter : IActionFilter
{
    private readonly ILogger<RootCauseAnalysisActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RootCauseAnalysisActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RootCauseAnalysisActionFilter(ILogger<RootCauseAnalysisActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("RootCauseAnalysis action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}