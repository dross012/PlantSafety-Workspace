// ============================================================================
// File: DOTActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for DOT endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DOTActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows DOT-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for DOT-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.DOT.Filters;

/// <summary>
/// Optional action filter for DOT. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class DOTActionFilter : IActionFilter
{
    private readonly ILogger<DOTActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="DOTActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public DOTActionFilter(ILogger<DOTActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("DOT action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}