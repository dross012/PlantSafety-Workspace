// ============================================================================
// File: WorkatHeightActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for WorkatHeight endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WorkatHeightActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows WorkatHeight-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for WorkatHeight-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.WorkatHeight.Filters;

/// <summary>
/// Optional action filter for WorkatHeight. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WorkatHeightActionFilter : IActionFilter
{
    private readonly ILogger<WorkatHeightActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WorkatHeightActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WorkatHeightActionFilter(ILogger<WorkatHeightActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("WorkatHeight action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}