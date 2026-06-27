// ============================================================================
// File: SafetyActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Safety endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(SafetyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Safety-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Safety-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.Safety.Filters;

/// <summary>
/// Optional action filter for Safety. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class SafetyActionFilter : IActionFilter
{
    private readonly ILogger<SafetyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="SafetyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public SafetyActionFilter(ILogger<SafetyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Safety action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}