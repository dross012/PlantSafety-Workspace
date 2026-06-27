// ============================================================================
// File: ConfinedSpaceActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ConfinedSpace endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ConfinedSpaceActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ConfinedSpace-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ConfinedSpace-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.ConfinedSpace.Filters;

/// <summary>
/// Optional action filter for ConfinedSpace. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ConfinedSpaceActionFilter : IActionFilter
{
    private readonly ILogger<ConfinedSpaceActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ConfinedSpaceActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ConfinedSpaceActionFilter(ILogger<ConfinedSpaceActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ConfinedSpace action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}