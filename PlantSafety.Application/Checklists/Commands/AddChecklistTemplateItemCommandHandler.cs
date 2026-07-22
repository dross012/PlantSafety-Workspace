// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for AddChecklistTemplateItemCommand. Pure orchestration: loads the ChecklistTemplate aggregate, invokes its AddItem() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="AddChecklistTemplateItemCommand"/>.</summary>
public sealed class AddChecklistTemplateItemCommandHandler : IRequestHandler<AddChecklistTemplateItemCommand, Result<AddChecklistTemplateItemResult>>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly ILogger<AddChecklistTemplateItemCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddChecklistTemplateItemCommandHandler(IChecklistTemplateRepository repository, ILogger<AddChecklistTemplateItemCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AddChecklistTemplateItemResult>> Handle(AddChecklistTemplateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistTemplateId.FromGuid(request.ChecklistTemplateId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AddChecklistTemplateItemResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistTemplate.NotFound",
                    "The requested ChecklistTemplate record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            var item = aggregate.AddItem(
                description: request.Description,
                isCritical: request.IsCritical,
                regulatoryReference: request.RegulatoryReference,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AddChecklistTemplateItemResult>.Success(new AddChecklistTemplateItemResult(
                aggregate.ChecklistTemplateId.Value,
                item.ItemId,
                item.Sequence));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add item to ChecklistTemplate {ChecklistTemplateId}", request.ChecklistTemplateId);

            return Result<AddChecklistTemplateItemResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.AddItemFailed",
                "Unable to add the checklist item at this time.",
                request.CorrelationId));
        }
    }
}
