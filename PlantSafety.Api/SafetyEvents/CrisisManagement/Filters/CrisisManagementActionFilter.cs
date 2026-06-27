// ============================================================================
// File: CrisisManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for CrisisManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CrisisManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows CrisisManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for CrisisManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.CrisisManagement.Filters;

/// <summary>
/// Optional action filter for CrisisManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CrisisManagementActionFilter : IActionFilter
{
    private readonly ILogger<CrisisManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CrisisManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CrisisManagementActionFilter(ILogger<CrisisManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("CrisisManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}