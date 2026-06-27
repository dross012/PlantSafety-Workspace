// ============================================================================
// File: PressureTestingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PressureTesting endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PressureTestingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PressureTesting-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PressureTesting-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.PressureTesting.Filters;

/// <summary>
/// Optional action filter for PressureTesting. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PressureTestingActionFilter : IActionFilter
{
    private readonly ILogger<PressureTestingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PressureTestingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PressureTestingActionFilter(ILogger<PressureTestingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PressureTesting action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}