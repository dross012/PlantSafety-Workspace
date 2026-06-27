// ============================================================================
// File: RiskAssessmentsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for RiskAssessments endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RiskAssessmentsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows RiskAssessments-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for RiskAssessments-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.ManagementOfChange.RiskAssessments.Filters;

/// <summary>
/// Optional action filter for RiskAssessments. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RiskAssessmentsActionFilter : IActionFilter
{
    private readonly ILogger<RiskAssessmentsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RiskAssessmentsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RiskAssessmentsActionFilter(ILogger<RiskAssessmentsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("RiskAssessments action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}