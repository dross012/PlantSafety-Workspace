// ============================================================================
// File: FinancialsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Financials endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(FinancialsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Financials-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Financials-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Financials.Filters;

/// <summary>
/// Optional action filter for Financials. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class FinancialsActionFilter : IActionFilter
{
    private readonly ILogger<FinancialsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="FinancialsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public FinancialsActionFilter(ILogger<FinancialsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Financials action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}