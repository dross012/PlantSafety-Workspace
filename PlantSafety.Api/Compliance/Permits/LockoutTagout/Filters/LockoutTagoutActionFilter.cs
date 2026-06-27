// ============================================================================
// File: LockoutTagoutActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for LockoutTagout endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(LockoutTagoutActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows LockoutTagout-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for LockoutTagout-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.LockoutTagout.Filters;

/// <summary>
/// Optional action filter for LockoutTagout. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class LockoutTagoutActionFilter : IActionFilter
{
    private readonly ILogger<LockoutTagoutActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="LockoutTagoutActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public LockoutTagoutActionFilter(ILogger<LockoutTagoutActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("LockoutTagout action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}