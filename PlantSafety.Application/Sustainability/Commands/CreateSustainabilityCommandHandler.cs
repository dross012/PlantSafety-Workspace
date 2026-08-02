// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CreateSustainabilityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateSustainabilityCommand. Pure orchestration: constructs the
//                  Sustainability aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.Commands;

/// <summary>Handles <see cref="CreateSustainabilityCommand"/>.</summary>
public sealed class CreateSustainabilityCommandHandler : IRequestHandler<CreateSustainabilityCommand, Result<CreateSustainabilityResult>>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<CreateSustainabilityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateSustainabilityCommandHandler(ISustainabilityRepository repository, ILogger<CreateSustainabilityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateSustainabilityResult>> Handle(CreateSustainabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerUserId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.Sustainability.Aggregates.Sustainability.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                initiativeName: request.InitiativeName,
                category: request.Category,
                targetValue: request.TargetValue,
                ownerUserId: ownerUserId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateSustainabilityResult>.Success(new CreateSustainabilityResult(
                aggregate.SustainabilityId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Sustainability initiative for Tenant {TenantId}", request.TenantId);

            return Result<CreateSustainabilityResult>.Failure(new ApplicationError(
                "Sustainability.Sustainability.CreateFailed",
                "Unable to submit the sustainability initiative at this time.",
                Guid.NewGuid()));
        }
    }
}
