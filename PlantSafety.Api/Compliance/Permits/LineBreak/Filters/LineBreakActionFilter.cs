// ============================================================================
// File: LineBreakActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for LineBreak endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(LineBreakActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows LineBreak-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for LineBreak-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.LineBreak.Filters;

/// <summary>
/// Optional action filter for LineBreak. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class LineBreakActionFilter : IActionFilter
{
    private readonly ILogger<LineBreakActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="LineBreakActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public LineBreakActionFilter(ILogger<LineBreakActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("LineBreak action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}