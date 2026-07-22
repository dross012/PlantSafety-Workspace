// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateChecklistTemplateCommand. Pure orchestration: constructs the ChecklistTemplate aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;
using PlantSafety.Domain.Checklists.SmartEnums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Handles <see cref="CreateChecklistTemplateCommand"/>.</summary>
public sealed class CreateChecklistTemplateCommandHandler : IRequestHandler<CreateChecklistTemplateCommand, Result<CreateChecklistTemplateResult>>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly ILogger<CreateChecklistTemplateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, ILogger<CreateChecklistTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateChecklistTemplateResult>> Handle(CreateChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var category = ChecklistCategory.IsValid(request.Category) ? ChecklistCategory.From(request.Category).Code : ChecklistCategory.Default.Code;

            var aggregate = PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                templateTitle: request.TemplateTitle,
                category: category,
                description: request.Description,
                itemCount: request.ItemCount,
                isMandatory: request.IsMandatory,
                regulatoryReference: request.RegulatoryReference);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateChecklistTemplateResult>.Success(new CreateChecklistTemplateResult(
                aggregate.ChecklistTemplateId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ChecklistTemplate for Tenant {TenantId}", request.TenantId);

            return Result<CreateChecklistTemplateResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.CreateFailed",
                "Unable to create the ChecklistTemplate record at this time.",
                Guid.NewGuid()));
        }
    }
}