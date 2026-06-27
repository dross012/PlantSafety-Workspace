// ============================================================================
// File: EmergencyActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Emergency endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EmergencyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Emergency-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Emergency-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Emergency.Filters;

/// <summary>
/// Optional action filter for Emergency. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EmergencyActionFilter : IActionFilter
{
    private readonly ILogger<EmergencyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EmergencyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EmergencyActionFilter(ILogger<EmergencyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Emergency action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}