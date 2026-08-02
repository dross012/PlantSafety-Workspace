// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           CreateHumanFactorsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateHumanFactorsCommand. Pure orchestration: constructs the
//                  HumanFactors aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.Commands;

/// <summary>Handles <see cref="CreateHumanFactorsCommand"/>.</summary>
public sealed class CreateHumanFactorsCommandHandler : IRequestHandler<CreateHumanFactorsCommand, Result<CreateHumanFactorsResult>>
{
    private readonly IHumanFactorsRepository _repository;
    private readonly ILogger<CreateHumanFactorsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateHumanFactorsCommandHandler(IHumanFactorsRepository repository, ILogger<CreateHumanFactorsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateHumanFactorsResult>> Handle(CreateHumanFactorsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var affectedUserId = request.AffectedUserId.HasValue ? UserId.FromGuid(request.AffectedUserId.Value) : (UserId?)null;

            var aggregate = PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                factorType: request.FactorType,
                affectedUserId: affectedUserId,
                severityRating: request.SeverityRating,
                description: request.Description);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateHumanFactorsResult>.Success(new CreateHumanFactorsResult(
                aggregate.HumanFactorsId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HumanFactors for Tenant {TenantId}", request.TenantId);

            return Result<CreateHumanFactorsResult>.Failure(new ApplicationError(
                "HumanFactors.CreateFailed",
                "Unable to submit the human-factors record at this time.",
                Guid.NewGuid()));
        }
    }
}
