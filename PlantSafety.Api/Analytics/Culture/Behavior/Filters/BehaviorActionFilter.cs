// ============================================================================
// File: BehaviorActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Behavior endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(BehaviorActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Behavior-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Behavior-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Culture.Behavior.Filters;

/// <summary>
/// Optional action filter for Behavior. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class BehaviorActionFilter : IActionFilter
{
    private readonly ILogger<BehaviorActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="BehaviorActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public BehaviorActionFilter(ILogger<BehaviorActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Behavior action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}