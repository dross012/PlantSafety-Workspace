// ============================================================================
// File: RiskManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for RiskManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RiskManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows RiskManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for RiskManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.RiskManagement.Filters;

/// <summary>
/// Optional action filter for RiskManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RiskManagementActionFilter : IActionFilter
{
    private readonly ILogger<RiskManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RiskManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RiskManagementActionFilter(ILogger<RiskManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("RiskManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}