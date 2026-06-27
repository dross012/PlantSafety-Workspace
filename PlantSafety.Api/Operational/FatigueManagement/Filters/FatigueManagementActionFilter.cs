// ============================================================================
// File: FatigueManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for FatigueManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(FatigueManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows FatigueManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for FatigueManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.FatigueManagement.Filters;

/// <summary>
/// Optional action filter for FatigueManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class FatigueManagementActionFilter : IActionFilter
{
    private readonly ILogger<FatigueManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="FatigueManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public FatigueManagementActionFilter(ILogger<FatigueManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("FatigueManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}