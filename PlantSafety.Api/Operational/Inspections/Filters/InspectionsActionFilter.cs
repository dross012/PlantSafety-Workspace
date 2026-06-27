// ============================================================================
// File: InspectionsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Inspections endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(InspectionsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Inspections-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Inspections-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Inspections.Filters;

/// <summary>
/// Optional action filter for Inspections. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class InspectionsActionFilter : IActionFilter
{
    private readonly ILogger<InspectionsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="InspectionsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public InspectionsActionFilter(ILogger<InspectionsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Inspections action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}