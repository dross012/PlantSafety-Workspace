// ============================================================================
// File: ApprovalsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Approvals endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ApprovalsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Approvals-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Approvals-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Approvals.Filters;

/// <summary>
/// Optional action filter for Approvals. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ApprovalsActionFilter : IActionFilter
{
    private readonly ILogger<ApprovalsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ApprovalsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ApprovalsActionFilter(ILogger<ApprovalsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Approvals action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}