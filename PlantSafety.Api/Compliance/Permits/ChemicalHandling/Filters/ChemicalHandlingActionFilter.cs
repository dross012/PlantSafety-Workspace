// ============================================================================
// File: ChemicalHandlingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ChemicalHandling endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ChemicalHandlingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ChemicalHandling-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ChemicalHandling-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.ChemicalHandling.Filters;

/// <summary>
/// Optional action filter for ChemicalHandling. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ChemicalHandlingActionFilter : IActionFilter
{
    private readonly ILogger<ChemicalHandlingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ChemicalHandlingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ChemicalHandlingActionFilter(ILogger<ChemicalHandlingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ChemicalHandling action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}