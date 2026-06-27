// ============================================================================
// File: HRActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HR endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HRActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HR-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HR-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.HR.Filters;

/// <summary>
/// Optional action filter for HR. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HRActionFilter : IActionFilter
{
    private readonly ILogger<HRActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HRActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HRActionFilter(ILogger<HRActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HR action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}