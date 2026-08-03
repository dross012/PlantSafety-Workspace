// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// LAST UPDATED: 2026-08-03 (Claude Code performance-optimization-agent) -- Handbook Phase 6 §5.4
//   Compliance Monitoring: every authorization decision (allow AND deny) is now recorded via
//   IPermissionUsageLogRepository, feeding the compliance/access-log audit trail. Also converted
//   the deny-path message-template warning log to a canonical structured envelope
//   (Handbook §2.19-2.22).
// FILE DESCRIPTION: MediatR pipeline behavior that re-enforces zero-trust authorization
//   for all requests decorated with IAuthorizedRequest.
//   Authorization is checked at the API boundary AND here; defense-in-depth.
//   Throws AuthorizationException on failure; never swallows unauthorized access.
// SchemaVersion: 2026.1
// Additive-only evolution: new authorization modes may be added; no checks may be bypassed.

using MediatR;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Abstractions.Security;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Application.Behaviors.Security;

/// <summary>
/// MediatR pipeline behavior that enforces zero-trust authorization for all requests
/// implementing <see cref="IAuthorizedRequest"/>.
/// Requests without <see cref="IAuthorizedRequest"/> pass through; authenticated-only
/// endpoints must implement the interface.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="AuthorizationBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="permissionService">The permission evaluation service.</param>
/// <param name="currentUserService">The current user identity service.</param>
/// <param name="operationContext">The current operation context carrying correlation lineage.</param>
/// <param name="applicationLogger">The canonical structured-envelope logger.</param>
/// <param name="permissionUsageLogRepository">Compliance-monitoring access-log capture (Handbook §5.4).</param>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    IPermissionService permissionService,
    ICurrentUserService currentUserService,
    IOperationContext operationContext,
    IApplicationLogger applicationLogger,
    IPermissionUsageLogRepository permissionUsageLogRepository) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Checks authorization for requests implementing <see cref="IAuthorizedRequest"/>
    /// and delegates to the next behavior on success.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ApplicationAuthorizationException">Thrown when the user lacks the required permission.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
        {
            return await next(cancellationToken);
        }

        if (!currentUserService.IsAuthenticated)
        {
            throw new ApplicationAuthorizationException(
                ErrorCodes.Unauthorized,
                "Authentication required.",
                operationContext.CorrelationId);
        }

        var hasPermission = await permissionService.HasPermissionAsync(
            currentUserService.UserId,
            operationContext.TenantId,
            authorizedRequest.RequiredPermission,
            cancellationToken);

        // Handbook Phase 6 §5.4 Compliance Monitoring: record every authorization decision, allow
        // or deny, for later access-log/compliance audit review. Best-effort -- never faults the
        // request pipeline (see PermissionUsageLogDapperRepository remarks).
        await permissionUsageLogRepository.RecordAsync(new PermissionUsageLogEntry(
            TenantId: operationContext.TenantId,
            UserId: currentUserService.UserId,
            Permission: authorizedRequest.RequiredPermission,
            RequestType: typeof(TRequest).Name,
            Decision: hasPermission ? "Allowed" : "Denied",
            CorrelationId: operationContext.CorrelationId,
            OccurredAtUtc: DateTime.UtcNow), cancellationToken);

        if (!hasPermission)
        {
            applicationLogger.LogWarning(new OperationLogEnvelope
            {
                OperationName = "Authorization.Denied",
                HandlerType = typeof(TRequest).Name,
                CorrelationId = operationContext.CorrelationId,
                CausationId = operationContext.CausationId,
                RequestId = operationContext.RequestId,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                TenantId = operationContext.TenantId,
                ActorId = currentUserService.UserId,
                TimestampUtc = DateTime.UtcNow,
                Success = false,
                Severity = LogSeverity.Warning
            });

            throw new ApplicationAuthorizationException(
                ErrorCodes.Forbidden,
                $"Permission '{authorizedRequest.RequiredPermission}' is required.",
                operationContext.CorrelationId);
        }

        return await next(cancellationToken);
    }
}

/// <summary>
/// Opt-in marker interface for requests that require a specific permission.
/// Implemented on Commands and Queries that must be authorized before execution.
/// </summary>
public interface IAuthorizedRequest
{
    /// <summary>Gets the fully-qualified permission name required to execute this request.</summary>
    string RequiredPermission { get; }
}
