// ============================================================================
// File: CleanupActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Cleanup endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CleanupActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Cleanup-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Cleanup-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Cleanup.Filters;

/// <summary>
/// Optional action filter for Cleanup. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CleanupActionFilter : IActionFilter
{
    private readonly ILogger<CleanupActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CleanupActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CleanupActionFilter(ILogger<CleanupActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Cleanup action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}