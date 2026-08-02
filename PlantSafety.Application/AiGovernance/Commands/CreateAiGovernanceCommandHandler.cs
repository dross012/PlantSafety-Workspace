// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           CreateAiGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateAiGovernanceCommand. Pure orchestration: constructs the
//                  AiGovernance aggregate via its domain factory method and persists it through the
//                  domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AiGovernance.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiGovernance.Commands;

/// <summary>Handles <see cref="CreateAiGovernanceCommand"/>.</summary>
public sealed class CreateAiGovernanceCommandHandler : IRequestHandler<CreateAiGovernanceCommand, Result<CreateAiGovernanceResult>>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly ILogger<CreateAiGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAiGovernanceCommandHandler(IAiGovernanceRepository repository, ILogger<CreateAiGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAiGovernanceResult>> Handle(CreateAiGovernanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                systemName: request.SystemName,
                aiUseCase: request.AiUseCase,
                riskClassification: request.RiskClassification,
                ownerUserId: ownerId,
                requiresHumanOversight: request.RequiresHumanOversight);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAiGovernanceResult>.Success(new CreateAiGovernanceResult(
                aggregate.AiGovernanceId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AiGovernance for Tenant {TenantId}", request.TenantId);

            return Result<CreateAiGovernanceResult>.Failure(new ApplicationError(
                "AiGovernance.CreateFailed",
                "Unable to register the AI governance record at this time.",
                Guid.NewGuid()));
        }
    }
}
