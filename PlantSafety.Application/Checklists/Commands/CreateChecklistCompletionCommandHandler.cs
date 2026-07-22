// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateChecklistCompletionCommand. Pure orchestration: constructs the ChecklistCompletion aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Handles <see cref="CreateChecklistCompletionCommand"/>.</summary>
public sealed class CreateChecklistCompletionCommandHandler : IRequestHandler<CreateChecklistCompletionCommand, Result<CreateChecklistCompletionResult>>
{
    private readonly IChecklistCompletionRepository _repository;
    private readonly ILogger<CreateChecklistCompletionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateChecklistCompletionCommandHandler(IChecklistCompletionRepository repository, ILogger<CreateChecklistCompletionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateChecklistCompletionResult>> Handle(CreateChecklistCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);


            var aggregate = PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                checklistTemplateId: request.ChecklistTemplateId,
                itemsTotal: request.ItemsTotal,
                itemsPassed: request.ItemsPassed,
                itemsFailed: request.ItemsFailed,
                outcome: request.Outcome);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateChecklistCompletionResult>.Success(new CreateChecklistCompletionResult(
                aggregate.ChecklistCompletionId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ChecklistCompletion for Tenant {TenantId}", request.TenantId);

            return Result<CreateChecklistCompletionResult>.Failure(new ApplicationError(
                "Checklists.ChecklistCompletion.CreateFailed",
                "Unable to create the ChecklistCompletion record at this time.",
                Guid.NewGuid()));
        }
    }
}