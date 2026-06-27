// ============================================================================
// File: PermissionsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Permissions endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PermissionsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Permissions-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Permissions-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Permissions.Filters;

/// <summary>
/// Optional action filter for Permissions. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PermissionsActionFilter : IActionFilter
{
    private readonly ILogger<PermissionsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PermissionsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PermissionsActionFilter(ILogger<PermissionsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Permissions action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}