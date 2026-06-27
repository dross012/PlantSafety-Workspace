// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that re-enforces zero-trust authorization
//   for all requests decorated with IAuthorizedRequest.
//   Authorization is checked at the API boundary AND here; defense-in-depth.
//   Throws AuthorizationException on failure; never swallows unauthorized access.
// SchemaVersion: 1
// Additive-only evolution: new authorization modes may be added; no checks may be bypassed.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Security;
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
/// <param name="logger">The structured logger.</param>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    IPermissionService permissionService,
    ICurrentUserService currentUserService,
    IOperationContext operationContext,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IPermissionService _permissionService = permissionService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IOperationContext _operationContext = operationContext;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger = logger;

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

        if (!_currentUserService.IsAuthenticated)
        {
            throw new ApplicationAuthorizationException(
                ErrorCodes.Unauthorized,
                "Authentication required.",
                _operationContext.CorrelationId);
        }

        var hasPermission = await _permissionService.HasPermissionAsync(
            _currentUserService.UserId,
            _operationContext.TenantId,
            authorizedRequest.RequiredPermission,
            cancellationToken);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "AuthorizationDenied | UserId={UserId} | Permission={Permission} | CorrelationId={CorrelationId}",
                _currentUserService.UserId,
                authorizedRequest.RequiredPermission,
                _operationContext.CorrelationId);

            throw new ApplicationAuthorizationException(
                ErrorCodes.Forbidden,
                $"Permission '{authorizedRequest.RequiredPermission}' is required.",
                _operationContext.CorrelationId);
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
