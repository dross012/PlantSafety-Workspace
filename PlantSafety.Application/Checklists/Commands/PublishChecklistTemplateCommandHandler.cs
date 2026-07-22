// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for PublishChecklistTemplateCommand. Pure orchestration: loads the ChecklistTemplate aggregate, invokes its Publish() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="PublishChecklistTemplateCommand"/>.</summary>
public sealed class PublishChecklistTemplateCommandHandler : IRequestHandler<PublishChecklistTemplateCommand, Result<PublishChecklistTemplateResult>>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly ILogger<PublishChecklistTemplateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, ILogger<PublishChecklistTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PublishChecklistTemplateResult>> Handle(PublishChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistTemplateId.FromGuid(request.ChecklistTemplateId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PublishChecklistTemplateResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistTemplate.NotFound",
                    "The requested ChecklistTemplate record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Publish(
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PublishChecklistTemplateResult>.Success(new PublishChecklistTemplateResult(
                aggregate.ChecklistTemplateId.Value,
                aggregate.Status.ToString(),
                aggregate.Items.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ChecklistTemplate {ChecklistTemplateId}", request.ChecklistTemplateId);

            return Result<PublishChecklistTemplateResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.PublishFailed",
                "Unable to publish the ChecklistTemplate at this time.",
                request.CorrelationId));
        }
    }
}
