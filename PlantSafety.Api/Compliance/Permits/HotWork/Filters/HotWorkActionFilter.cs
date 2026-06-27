// ============================================================================
// File: HotWorkActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HotWork endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HotWorkActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HotWork-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HotWork-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.HotWork.Filters;

/// <summary>
/// Optional action filter for HotWork. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HotWorkActionFilter : IActionFilter
{
    private readonly ILogger<HotWorkActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HotWorkActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HotWorkActionFilter(ILogger<HotWorkActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HotWork action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}