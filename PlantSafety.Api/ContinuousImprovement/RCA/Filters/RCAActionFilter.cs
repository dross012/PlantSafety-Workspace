// ============================================================================
// File: RCAActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for RCA endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RCAActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows RCA-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for RCA-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.ContinuousImprovement.RCA.Filters;

/// <summary>
/// Optional action filter for RCA. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RCAActionFilter : IActionFilter
{
    private readonly ILogger<RCAActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RCAActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RCAActionFilter(ILogger<RCAActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("RCA action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}