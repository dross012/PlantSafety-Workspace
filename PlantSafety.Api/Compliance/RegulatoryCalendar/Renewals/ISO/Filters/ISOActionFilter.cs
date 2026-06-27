// ============================================================================
// File: ISOActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ISO endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ISOActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ISO-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ISO-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Renewals.ISO.Filters;

/// <summary>
/// Optional action filter for ISO. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ISOActionFilter : IActionFilter
{
    private readonly ILogger<ISOActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ISOActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ISOActionFilter(ILogger<ISOActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ISO action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}