// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RetireChecklistTemplateCommand. Pure orchestration: loads the ChecklistTemplate aggregate, invokes its Retire() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;
using PlantSafety.Domain.Checklists.ChecklistTemplate.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Handles <see cref="RetireChecklistTemplateCommand"/>.</summary>
public sealed class RetireChecklistTemplateCommandHandler : IRequestHandler<RetireChecklistTemplateCommand, Result<RetireChecklistTemplateResult>>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly ILogger<RetireChecklistTemplateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, ILogger<RetireChecklistTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RetireChecklistTemplateResult>> Handle(RetireChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistTemplateId.FromGuid(request.ChecklistTemplateId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RetireChecklistTemplateResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistTemplate.NotFound",
                    "The requested ChecklistTemplate record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Retire(
                reason: request.Reason,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RetireChecklistTemplateResult>.Success(new RetireChecklistTemplateResult(
                aggregate.ChecklistTemplateId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire ChecklistTemplate {ChecklistTemplateId}", request.ChecklistTemplateId);

            return Result<RetireChecklistTemplateResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.RetireFailed",
                "Unable to retire the ChecklistTemplate at this time.",
                request.CorrelationId));
        }
    }
}
