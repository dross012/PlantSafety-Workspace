// ============================================================================
// File: CAPAActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for CAPA endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CAPAActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows CAPA-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for CAPA-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.ContinuousImprovement.CAPA.Filters;

/// <summary>
/// Optional action filter for CAPA. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CAPAActionFilter : IActionFilter
{
    private readonly ILogger<CAPAActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CAPAActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CAPAActionFilter(ILogger<CAPAActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("CAPA action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}