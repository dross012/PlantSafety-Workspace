// ============================================================================
// File: AuditingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Auditing endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AuditingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Auditing-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Auditing-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Auditing.Filters;

/// <summary>
/// Optional action filter for Integrations.Auditing. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AuditingActionFilter : IActionFilter
{
    private readonly ILogger<AuditingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AuditingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AuditingActionFilter(ILogger<AuditingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Auditing action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
