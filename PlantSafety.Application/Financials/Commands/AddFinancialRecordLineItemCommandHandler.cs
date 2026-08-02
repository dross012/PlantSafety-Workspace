// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           AddFinancialRecordLineItemCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AddFinancialRecordLineItemCommand. GetByIdAsync is tenant-scoped,
//                  the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Financials.Interfaces;
using PlantSafety.Domain.Financials.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Financials.Commands;

/// <summary>Handles <see cref="AddFinancialRecordLineItemCommand"/>.</summary>
public sealed class AddFinancialRecordLineItemCommandHandler : IRequestHandler<AddFinancialRecordLineItemCommand, Result<FinancialRecordStatusResult>>
{
    private readonly IFinancialRecordRepository _repository;
    private readonly ILogger<AddFinancialRecordLineItemCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddFinancialRecordLineItemCommandHandler(IFinancialRecordRepository repository, ILogger<AddFinancialRecordLineItemCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FinancialRecordStatusResult>> Handle(AddFinancialRecordLineItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = FinancialsId.FromGuid(request.FinancialRecordId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<FinancialRecordStatusResult>.Failure(new ApplicationError(
                    "Financials.FinancialRecord.NotFound",
                    "The requested financial record could not be found.",
                    request.FinancialRecordId,
                    ErrorSeverity.NotFound));
            }

            var actorUserId = UserId.FromGuid(request.ActorUserId);

            aggregate.AddLineItem(
                description: request.Description,
                amount: request.Amount,
                actor: PerformedByActor.User(actorUserId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<FinancialRecordStatusResult>.Success(new FinancialRecordStatusResult(
                aggregate.FinancialsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add line item to FinancialRecord {FinancialRecordId}", request.FinancialRecordId);

            return Result<FinancialRecordStatusResult>.Failure(new ApplicationError(
                "Financials.FinancialRecord.AddLineItemFailed",
                "Unable to add the line item at this time.",
                Guid.NewGuid()));
        }
    }
}
