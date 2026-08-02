// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           CloseFinancialRecordCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseFinancialRecordCommand. Mirrors
//                  SubmitFinancialRecordCommandHandler.
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

/// <summary>Handles <see cref="CloseFinancialRecordCommand"/>.</summary>
public sealed class CloseFinancialRecordCommandHandler : IRequestHandler<CloseFinancialRecordCommand, Result<FinancialRecordStatusResult>>
{
    private readonly IFinancialRecordRepository _repository;
    private readonly ILogger<CloseFinancialRecordCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseFinancialRecordCommandHandler(IFinancialRecordRepository repository, ILogger<CloseFinancialRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FinancialRecordStatusResult>> Handle(CloseFinancialRecordCommand request, CancellationToken cancellationToken)
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

            aggregate.Close(
                closedBy: actorUserId,
                closedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to close FinancialRecord {FinancialRecordId}", request.FinancialRecordId);

            return Result<FinancialRecordStatusResult>.Failure(new ApplicationError(
                "Financials.FinancialRecord.CloseFailed",
                "Unable to close the financial record at this time.",
                Guid.NewGuid()));
        }
    }
}
