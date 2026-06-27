// ============================================================================
// File: AuditFindingsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for AuditFindings endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AuditFindingsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows AuditFindings-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for AuditFindings-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Auditing.AuditFindings.Filters;

/// <summary>
/// Optional action filter for AuditFindings. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AuditFindingsActionFilter : IActionFilter
{
    private readonly ILogger<AuditFindingsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AuditFindingsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AuditFindingsActionFilter(ILogger<AuditFindingsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("AuditFindings action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}