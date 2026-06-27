// ============================================================================
// File: CAPATriggersActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for CAPATriggers endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CAPATriggersActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows CAPATriggers-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for CAPATriggers-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Auditing.CAPATriggers.Filters;

/// <summary>
/// Optional action filter for CAPATriggers. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CAPATriggersActionFilter : IActionFilter
{
    private readonly ILogger<CAPATriggersActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CAPATriggersActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CAPATriggersActionFilter(ILogger<CAPATriggersActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("CAPATriggers action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}