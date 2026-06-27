// ============================================================================
// File: ElectricalActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Electrical endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ElectricalActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Electrical-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Electrical-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Electrical.Filters;

/// <summary>
/// Optional action filter for Electrical. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ElectricalActionFilter : IActionFilter
{
    private readonly ILogger<ElectricalActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ElectricalActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ElectricalActionFilter(ILogger<ElectricalActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Electrical action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}