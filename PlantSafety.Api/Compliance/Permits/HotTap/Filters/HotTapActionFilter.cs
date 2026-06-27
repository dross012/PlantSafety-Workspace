// ============================================================================
// File: HotTapActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HotTap endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HotTapActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HotTap-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HotTap-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.HotTap.Filters;

/// <summary>
/// Optional action filter for HotTap. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HotTapActionFilter : IActionFilter
{
    private readonly ILogger<HotTapActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HotTapActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HotTapActionFilter(ILogger<HotTapActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HotTap action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}