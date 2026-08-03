// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           WorkflowInstanceLockBehavior.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    MediatR pipeline behavior serializing concurrent mutation of a single
//                  WorkflowInstance aggregate via a SQL Server sp_getapplock-backed distributed
//                  lock (IDistributedLockProvider). Applies ONLY to commands implementing
//                  IWorkflowInstanceScopedCommand; every other request passes through untouched.
//
//                  Registered immediately before TransactionBehavior in
//                  ApplicationServiceRegistration.AddApplicationServices, so the lock is acquired
//                  before the Unit of Work transaction begins and released only after that
//                  transaction (and the handler's full load-mutate-persist cycle: GetByIdAsync,
//                  apply the transition, UpdateAsync) has completed -- eliminating the race where
//                  two concurrent commands against the same WorkflowInstanceId both read the same
//                  pre-mutation state before either writes back.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Workflow.Commands;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Application.Workflow.Behaviors;

/// <summary>
/// MediatR pipeline behavior that serializes concurrent mutation of a single
/// <c>WorkflowInstance</c> aggregate. Only executes for requests implementing
/// <see cref="IWorkflowInstanceScopedCommand"/>; all other requests pass through without
/// acquiring a lock.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="WorkflowInstanceLockBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="lockProvider">The distributed lock provider (SQL Server sp_getapplock-backed).</param>
/// <param name="logger">The structured logger.</param>
public sealed class WorkflowInstanceLockBehavior<TRequest, TResponse>(
    IDistributedLockProvider lockProvider,
    ILogger<WorkflowInstanceLockBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Maximum time to wait for the WorkflowInstance lock before failing the command. Workflow
    /// step transitions are short-lived (a single stored-procedure round trip), so ten seconds is
    /// generous headroom while still failing fast under genuine contention or deadlock.
    /// </summary>
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Acquires an exclusive distributed lock keyed by the command's WorkflowInstanceId before
    /// invoking the next behavior/handler, and releases it once that call completes (success or
    /// failure). Requests that do not implement <see cref="IWorkflowInstanceScopedCommand"/> pass
    /// straight through.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IWorkflowInstanceScopedCommand scoped)
        {
            return await next(cancellationToken);
        }

        var resourceKey = $"Workflow:Instance:{scoped.WorkflowInstanceId:D}";

        await using var lockHandle = await lockProvider.AcquireAsync(resourceKey, LockTimeout, cancellationToken);

        logger.LogDebug(
            "WorkflowInstanceLockAcquired | ResourceKey={ResourceKey} | RequestType={RequestType}",
            resourceKey, typeof(TRequest).Name);

        return await next(cancellationToken);
    }
}
