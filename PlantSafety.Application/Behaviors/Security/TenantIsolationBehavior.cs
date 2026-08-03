// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: MediatR pipeline behavior that closes a platform-wide cross-tenant data
//   leakage exposure. ~148 Get*Request API DTOs bind a client-supplied [FromQuery]/[FromBody]
//   `TenantId` that flows unchecked into the Command/Query used for data filtering, while
//   AuthorizationBehavior only authorizes against the server-resolved IOperationContext.TenantId
//   (from the JWT claim via TenantResolutionMiddleware/RequestEnvelopeFactory) -- never against
//   request.TenantId. A malicious/misconfigured client can therefore pass an arbitrary TenantId
//   and read or act on another tenant's data.
//   Fixing every DTO/Command/Query record individually (~148 files) would be slow and
//   error-prone, so this behavior enforces isolation centrally: for ANY request that exposes a
//   public `Guid TenantId` (or `Guid? TenantId`) property -- the near-universal convention across
//   this codebase's Commands/Queries -- its value is compared against the server-resolved
//   IOperationContext.TenantId. Any mismatch is rejected before the handler runs, regardless of
//   which controller or DTO produced the request.
//   This is defense-in-depth alongside (not a replacement for) fixing individual controllers to
//   stop trusting client-supplied TenantId values over time.
// SchemaVersion: 1
// Additive-only evolution: no checks may be bypassed or weakened.

using System.Collections.Concurrent;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Application.Behaviors.Security;

/// <summary>
/// MediatR pipeline behavior that rejects any request whose bound <c>TenantId</c> property does
/// not match the server-resolved <see cref="IOperationContext.TenantId"/>, preventing cross-tenant
/// data access via a client-supplied TenantId value.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="TenantIsolationBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="operationContext">The current operation context carrying the server-resolved tenant.</param>
/// <param name="currentUserService">The current user identity service, for security logging.</param>
/// <param name="logger">The structured logger.</param>
public sealed class TenantIsolationBehavior<TRequest, TResponse>(
    IOperationContext operationContext,
    ICurrentUserService currentUserService,
    ILogger<TenantIsolationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> TenantIdPropertyCache = new();

    private readonly IOperationContext _operationContext = operationContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<TenantIsolationBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Rejects the request with <see cref="ApplicationAuthorizationException"/> when its bound
    /// <c>TenantId</c> property does not match the server-resolved tenant context.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ApplicationAuthorizationException">Thrown when the request's TenantId does not match the server-resolved tenant.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantIdProperty = TenantIdPropertyCache.GetOrAdd(request.GetType(), FindTenantIdProperty);

        if (tenantIdProperty is not null)
        {
            // A boxed `Guid?` that has a value boxes identically to a boxed `Guid`, so a single
            // `Guid` pattern covers both the non-nullable and the nullable-with-value case; a
            // `null` `Guid?` boxes to a null reference and falls through to the default arm.
            var requestTenantId = tenantIdProperty.GetValue(request) switch
            {
                Guid guid => guid,
                _ => (Guid?)null
            };

            if (requestTenantId.HasValue && requestTenantId.Value != _operationContext.TenantId)
            {
                _logger.LogWarning(
                    "CrossTenantAccessAttemptBlocked | UserId={UserId} | RequestType={RequestType} | " +
                    "ResolvedTenantId={ResolvedTenantId} | RequestedTenantId={RequestedTenantId} | CorrelationId={CorrelationId}",
                    _currentUserService.UserId,
                    typeof(TRequest).FullName,
                    _operationContext.TenantId,
                    requestTenantId.Value,
                    _operationContext.CorrelationId);

                throw new ApplicationAuthorizationException(
                    ErrorCodes.TenantMismatch,
                    "The request's TenantId does not match the authenticated tenant context.",
                    _operationContext.CorrelationId);
            }
        }

        return await next(cancellationToken);
    }

    /// <summary>
    /// Locates a public instance <c>TenantId</c> property of type <see cref="Guid"/> or <see cref="Nullable{Guid}"/>
    /// declared directly on the request type, if any.
    /// </summary>
    private static PropertyInfo? FindTenantIdProperty(Type requestType)
    {
        var property = requestType.GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance);

        return property is not null && (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
            ? property
            : null;
    }
}
