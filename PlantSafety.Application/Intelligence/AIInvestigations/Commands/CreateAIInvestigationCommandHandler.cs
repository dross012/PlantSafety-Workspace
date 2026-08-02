// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           CreateAIInvestigationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateAIInvestigationCommand. Pure orchestration: constructs the
//                  AIInvestigation aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Commands;

/// <summary>Handles <see cref="CreateAIInvestigationCommand"/>.</summary>
public sealed class CreateAIInvestigationCommandHandler : IRequestHandler<CreateAIInvestigationCommand, Result<AIInvestigationStatusResult>>
{
    private readonly IAIInvestigationRepository _repository;
    private readonly ILogger<CreateAIInvestigationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAIInvestigationCommandHandler(IAIInvestigationRepository repository, ILogger<CreateAIInvestigationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AIInvestigationStatusResult>> Handle(CreateAIInvestigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var assignedTo = UserId.FromGuid(request.AssignedToUserId);

            var aggregate = PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation.Create(
                createdBy: assignedTo,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(assignedTo),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentReference: request.IncidentReference,
                aiConfidenceScore: request.AiConfidenceScore,
                summary: request.Summary,
                assignedTo: assignedTo);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AIInvestigationStatusResult>.Success(new AIInvestigationStatusResult(
                aggregate.AIInvestigationId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AIInvestigation for Tenant {TenantId}", request.TenantId);

            return Result<AIInvestigationStatusResult>.Failure(new ApplicationError(
                "Intelligence.AIInvestigation.CreateFailed",
                "Unable to open the AI investigation at this time.",
                Guid.NewGuid()));
        }
    }
}
