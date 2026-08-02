// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           CreateRCACommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateRCACommand. Pure orchestration: constructs the RCA aggregate
//                  via its domain factory method and persists it through the domain repository
//                  interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Handles <see cref="CreateRCACommand"/>.</summary>
public sealed class CreateRCACommandHandler : IRequestHandler<CreateRCACommand, Result<RCAStatusResult>>
{
    private readonly IRCARepository _repository;
    private readonly ILogger<CreateRCACommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRCACommandHandler(IRCARepository repository, ILogger<CreateRCACommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCAStatusResult>> Handle(CreateRCACommand request, CancellationToken cancellationToken)
    {
        try
        {
            var owner = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA.Create(
                createdBy: owner,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(owner),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                problemStatement: request.ProblemStatement,
                methodology: request.Methodology,
                ownerUserId: owner);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RCAStatusResult>.Success(new RCAStatusResult(
                aggregate.RcaId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RCA for Tenant {TenantId}", request.TenantId);

            return Result<RCAStatusResult>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.CreateFailed",
                "Unable to create the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
