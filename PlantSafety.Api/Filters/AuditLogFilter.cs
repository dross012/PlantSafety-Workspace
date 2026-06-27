// ============================================================================
// File: AuditLogFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: IAsyncActionFilter that emits a structured audit log entry for
//              every state-mutating (POST, PUT, PATCH, DELETE) API request. Read
//              requests (GET, HEAD, OPTIONS) are skipped.
// ============================================================================
//
// Onboarding Notes:
// - Registered globally in ServiceCollectionExtensions.AddPlantSafetyApi() via
//   options.Filters.Add<AuditLogFilter>() so every controller action is covered
//   without per-controller decoration.
// - Uses ILogger<AuditLogFilter> to emit a structured log with scoped context.
//   The Infrastructure layer's Serilog sink is responsible for routing these log
//   entries to the audit store — the filter itself has no Infrastructure dependency.
// - AuditEntry is emitted AFTER the action completes so the HTTP status code is
//   available. Entries for 5xx responses include IsSuccess=false to support
//   compliance dashboards that track failed mutation attempts.
// - TenantId and CorrelationId are sourced from HttpContext.Items via the
//   CrossDomain accessors to avoid duplicating extraction logic.
//
// Rationale:
// - Centralised audit logging ensures that no state-mutating endpoint is omitted
//   from the audit trail without an explicit opt-out attribute decoration.
//
// Future Extension Seams:
// - Add [SkipAuditLog] attribute to allow opt-out on specific actions.
// - Add RequestBody capture (with PII scrubbing) for high-sensitivity modules
//   such as Compliance.AuditFindings.
//
// ============================================================================

using PlantSafety.Api.CrossDomain;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Globally registered action filter that emits a structured audit log entry
/// for every state-mutating HTTP request (POST, PUT, PATCH, DELETE).
/// </summary>
public sealed class AuditLogFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> MutatingMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

    private readonly ILogger<AuditLogFilter> _logger;
    private readonly TenantContextAccessor  _tenantContext;
    private readonly CorrelationIdAccessor  _correlationId;

    /// <summary>
    /// Initialises a new instance of <see cref="AuditLogFilter"/>.
    /// </summary>
    public AuditLogFilter(
        ILogger<AuditLogFilter>  logger,
        TenantContextAccessor    tenantContext,
        CorrelationIdAccessor    correlationId)
    {
        _logger        = logger;
        _tenantContext = tenantContext;
        _correlationId = correlationId;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(
        ActionExecutingContext  context,
        ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;

        if (!MutatingMethods.Contains(method))
        {
            await next();
            return;
        }

        var executed = await next();

        var statusCode = executed.HttpContext.Response.StatusCode;

        _logger.LogInformation(
            "AUDIT | TenantId={TenantId} CorrelationId={CorrelationId} " +
            "Method={Method} Path={Path} StatusCode={StatusCode} " +
            "Action={Action} IsSuccess={IsSuccess} UserId={UserId}",
            _tenantContext.TenantId,
            _correlationId.CorrelationId,
            method,
            context.HttpContext.Request.Path,
            statusCode,
            context.ActionDescriptor.DisplayName,
            statusCode < 400,
            context.HttpContext.User.Identity?.Name ?? "anonymous");
    }
}
