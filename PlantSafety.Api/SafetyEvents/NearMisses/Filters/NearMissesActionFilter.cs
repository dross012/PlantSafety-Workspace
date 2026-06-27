// ============================================================================
// File: NearMissesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for NearMisses endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(NearMissesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows NearMisses-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for NearMisses-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.NearMisses.Filters;

/// <summary>
/// Optional action filter for NearMisses. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class NearMissesActionFilter : IActionFilter
{
    private readonly ILogger<NearMissesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="NearMissesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public NearMissesActionFilter(ILogger<NearMissesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("NearMisses action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}