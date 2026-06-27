// ============================================================================
// File: LiftingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Lifting endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(LiftingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Lifting-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Lifting-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Lifting.Filters;

/// <summary>
/// Optional action filter for Lifting. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class LiftingActionFilter : IActionFilter
{
    private readonly ILogger<LiftingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="LiftingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public LiftingActionFilter(ILogger<LiftingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Lifting action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}