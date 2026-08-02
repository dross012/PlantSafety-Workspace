// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           ActivateContractorVisitorCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateContractorVisitorCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="ActivateContractorVisitorCommand"/>.</summary>
public sealed class ActivateContractorVisitorCommandHandler : IRequestHandler<ActivateContractorVisitorCommand, Result<ContractorVisitorStatusResult>>
{
    private readonly IContractorVisitorRepository _repository;
    private readonly ILogger<ActivateContractorVisitorCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateContractorVisitorCommandHandler(IContractorVisitorRepository repository, ILogger<ActivateContractorVisitorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContractorVisitorStatusResult>> Handle(ActivateContractorVisitorCommand request, CancellationToken cancellationToken)
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

            aggregate.Activate(
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
            _logger.LogError(ex, "Failed to activate ContractorVisitor {ContractorVisitorId}", request.ContractorVisitorId);

            return Result<ContractorVisitorStatusResult>.Failure(new ApplicationError(
                "ContractorVisitor.ActivateFailed",
                "Unable to activate the contractor/visitor record at this time.",
                Guid.NewGuid()));
        }
    }
}
