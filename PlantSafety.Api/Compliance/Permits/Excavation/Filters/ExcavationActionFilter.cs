// ============================================================================
// File: ExcavationActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Excavation endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ExcavationActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Excavation-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Excavation-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Excavation.Filters;

/// <summary>
/// Optional action filter for Excavation. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ExcavationActionFilter : IActionFilter
{
    private readonly ILogger<ExcavationActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ExcavationActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ExcavationActionFilter(ILogger<ExcavationActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Excavation action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}