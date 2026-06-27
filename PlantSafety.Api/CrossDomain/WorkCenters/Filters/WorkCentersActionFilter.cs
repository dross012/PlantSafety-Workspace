// ============================================================================
// File: WorkCentersActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for WorkCenters endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WorkCentersActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows WorkCenters-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for WorkCenters-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.WorkCenters.Filters;

/// <summary>
/// Optional action filter for WorkCenters. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WorkCentersActionFilter : IActionFilter
{
    private readonly ILogger<WorkCentersActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WorkCentersActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WorkCentersActionFilter(ILogger<WorkCentersActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("WorkCenters action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}