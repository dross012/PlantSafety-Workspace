// ============================================================================
// File: ComplianceAuditActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ComplianceAudit endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ComplianceAuditActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ComplianceAudit-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ComplianceAudit-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Auditing.ComplianceAudit.Filters;

/// <summary>
/// Optional action filter for ComplianceAudit. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ComplianceAuditActionFilter : IActionFilter
{
    private readonly ILogger<ComplianceAuditActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ComplianceAuditActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ComplianceAuditActionFilter(ILogger<ComplianceAuditActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ComplianceAudit action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}