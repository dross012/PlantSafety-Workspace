// ============================================================================
// File: RadiographyActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Radiography endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RadiographyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Radiography-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Radiography-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Radiography.Filters;

/// <summary>
/// Optional action filter for Radiography. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RadiographyActionFilter : IActionFilter
{
    private readonly ILogger<RadiographyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RadiographyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RadiographyActionFilter(ILogger<RadiographyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Radiography action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}