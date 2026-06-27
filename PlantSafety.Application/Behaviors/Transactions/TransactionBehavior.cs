// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that wraps Command execution in a Unit of Work transaction.
//   Executes FOURTH in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Applies ONLY to ICommand requests; IQuery requests pass through without a transaction.
//   The Unit of Work guarantees Publish-After-Commit semantics — no lost or phantom events (§6).
// SchemaVersion: 1
// Additive-only evolution: new retry/rollback fields may be added; no transaction semantics may be altered.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Behaviors.Transactions;

/// <summary>
/// MediatR pipeline behavior that wraps command execution in a Unit of Work transaction.
/// Queries bypass this behavior entirely.
/// On success: commits the transaction and triggers the Outbox Processor.
/// On failure: rolls back and emits a canonical error envelope.
/// Position in pipeline: fourth (after Logging, Performance, Validation; wraps the Handler).
/// </summary>
/// <typeparam name="TRequest">The MediatR request type. Only ICommand types are transacted.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="TransactionBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="logger">The structured logger.</param>
/// <param name="operationContext">The current operation context carrying correlation lineage.</param>
/// <param name="unitOfWork">The Unit of Work that owns the SQL transaction and outbox commit.</param>
public sealed class TransactionBehavior<TRequest, TResponse>(
    ILogger<TransactionBehavior<TRequest, TResponse>> logger,
    IOperationContext operationContext,
    IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IOperationContext _operationContext = operationContext;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Wraps command execution in a Unit of Work transaction.
    /// Query requests bypass this behavior and pass directly to the next delegate.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="Exception">Re-throws after rollback on command failure.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand and not ICommand<TResponse>)
        {
            return await next(cancellationToken);
        }

        try
        {
            await _unitOfWork.BeginAsync(cancellationToken);
            var response = await next(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "TransactionCommitted | RequestType={RequestType} | CorrelationId={CorrelationId}",
                typeof(TRequest).Name,
                _operationContext.CorrelationId);

            return response;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);

            _logger.LogError(
                ex,
                "TransactionRolledBack | RequestType={RequestType} | CorrelationId={CorrelationId} | ExceptionType={ExceptionType}",
                typeof(TRequest).Name,
                _operationContext.CorrelationId,
                ex.GetType().Name);

            throw;
        }
    }
}
