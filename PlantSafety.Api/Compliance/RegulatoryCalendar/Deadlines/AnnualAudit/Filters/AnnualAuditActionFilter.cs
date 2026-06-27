// ============================================================================
// File: AnnualAuditActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for AnnualAudit endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AnnualAuditActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows AnnualAudit-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for AnnualAudit-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Filters;

/// <summary>
/// Optional action filter for AnnualAudit. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AnnualAuditActionFilter : IActionFilter
{
    private readonly ILogger<AnnualAuditActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AnnualAuditActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AnnualAuditActionFilter(ILogger<AnnualAuditActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("AnnualAudit action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}