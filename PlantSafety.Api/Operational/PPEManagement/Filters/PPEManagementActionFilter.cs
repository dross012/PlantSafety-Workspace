// ============================================================================
// File: PPEManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PPEManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PPEManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PPEManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PPEManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.PPEManagement.Filters;

/// <summary>
/// Optional action filter for PPEManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PPEManagementActionFilter : IActionFilter
{
    private readonly ILogger<PPEManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PPEManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PPEManagementActionFilter(ILogger<PPEManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PPEManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}