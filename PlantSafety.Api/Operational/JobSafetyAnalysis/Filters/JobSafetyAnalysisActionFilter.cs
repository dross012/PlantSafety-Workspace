// ============================================================================
// File: JobSafetyAnalysisActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for JobSafetyAnalysis endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(JobSafetyAnalysisActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows JobSafetyAnalysis-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for JobSafetyAnalysis-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.JobSafetyAnalysis.Filters;

/// <summary>
/// Optional action filter for JobSafetyAnalysis. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class JobSafetyAnalysisActionFilter : IActionFilter
{
    private readonly ILogger<JobSafetyAnalysisActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="JobSafetyAnalysisActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public JobSafetyAnalysisActionFilter(ILogger<JobSafetyAnalysisActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("JobSafetyAnalysis action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}