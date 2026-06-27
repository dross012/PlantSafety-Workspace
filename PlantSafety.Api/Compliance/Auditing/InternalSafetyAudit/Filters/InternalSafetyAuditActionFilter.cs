// ============================================================================
// File: InternalSafetyAuditActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for InternalSafetyAudit endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(InternalSafetyAuditActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows InternalSafetyAudit-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for InternalSafetyAudit-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Filters;

/// <summary>
/// Optional action filter for InternalSafetyAudit. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class InternalSafetyAuditActionFilter : IActionFilter
{
    private readonly ILogger<InternalSafetyAuditActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="InternalSafetyAuditActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public InternalSafetyAuditActionFilter(ILogger<InternalSafetyAuditActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("InternalSafetyAudit action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}