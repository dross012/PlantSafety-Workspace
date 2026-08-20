// ============================================================================
// File: WorkforceSafetyActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Optional per-sub-area action filter for WorkforceSafety endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WorkforceSafetyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows WorkforceSafety-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for WorkforceSafety-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.WorkforceSafety.Filters;

/// <summary>
/// Optional action filter for WorkforceSafety. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WorkforceSafetyActionFilter : IActionFilter
{
    private readonly ILogger<WorkforceSafetyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WorkforceSafetyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WorkforceSafetyActionFilter(ILogger<WorkforceSafetyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("WorkforceSafety action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}