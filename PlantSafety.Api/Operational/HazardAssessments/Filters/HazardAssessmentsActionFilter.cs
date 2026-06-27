// ============================================================================
// File: HazardAssessmentsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HazardAssessments endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HazardAssessmentsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HazardAssessments-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HazardAssessments-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.HazardAssessments.Filters;

/// <summary>
/// Optional action filter for HazardAssessments. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HazardAssessmentsActionFilter : IActionFilter
{
    private readonly ILogger<HazardAssessmentsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HazardAssessmentsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HazardAssessmentsActionFilter(ILogger<HazardAssessmentsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HazardAssessments action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}