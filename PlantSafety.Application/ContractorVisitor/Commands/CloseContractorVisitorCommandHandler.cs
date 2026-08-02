// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           CloseContractorVisitorCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseContractorVisitorCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorVisitor.Repositories;
using PlantSafety.Domain.ContractorVisitor.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorVisitor.Commands;

/// <summary>Handles <see cref="CloseContractorVisitorCommand"/>.</summary>
public sealed class CloseContractorVisitorCommandHandler : IRequestHandler<CloseContractorVisitorCommand, Result<ContractorVisitorStatusResult>>
{
    private readonly IContractorVisitorRepository _repository;
    private readonly ILogger<CloseContractorVisitorCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseContractorVisitorCommandHandler(IContractorVisitorRepository repository, ILogger<CloseContractorVisitorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContractorVisitorStatusResult>> Handle(CloseContractorVisitorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ContractorVisitorId.FromGuid(request.ContractorVisitorId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ContractorVisitorStatusResult>.Failure(new ApplicationError(
                    "ContractorVisitor.NotFound",
                    "The requested contractor/visitor record could not be found.",
                    request.ContractorVisitorId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ContractorVisitorStatusResult>.Success(new ContractorVisitorStatusResult(
                aggregate.ContractorVisitorId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close ContractorVisitor {ContractorVisitorId}", request.ContractorVisitorId);

            return Result<ContractorVisitorStatusResult>.Failure(new ApplicationError(
                "ContractorVisitor.CloseFailed",
                "Unable to close the contractor/visitor record at this time.",
                Guid.NewGuid()));
        }
    }
}
