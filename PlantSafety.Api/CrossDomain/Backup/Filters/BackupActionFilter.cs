// ============================================================================
// File: BackupActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Backup endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(BackupActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Backup-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Backup-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Backup.Filters;

/// <summary>
/// Optional action filter for Backup. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class BackupActionFilter : IActionFilter
{
    private readonly ILogger<BackupActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="BackupActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public BackupActionFilter(ILogger<BackupActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Backup action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}