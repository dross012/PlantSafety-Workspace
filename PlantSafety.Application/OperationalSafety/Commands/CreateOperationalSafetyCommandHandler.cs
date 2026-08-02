// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CreateOperationalSafetyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateOperationalSafetyCommand. Pure orchestration: constructs
//                  the OperationalSafety aggregate via its domain factory method and persists
//                  it through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.OperationalSafety.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Handles <see cref="CreateOperationalSafetyCommand"/>.</summary>
public sealed class CreateOperationalSafetyCommandHandler : IRequestHandler<CreateOperationalSafetyCommand, Result<CreateOperationalSafetyResult>>
{
    private readonly IOperationalSafetyRepository _repository;
    private readonly ILogger<CreateOperationalSafetyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateOperationalSafetyCommandHandler(IOperationalSafetyRepository repository, ILogger<CreateOperationalSafetyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateOperationalSafetyResult>> Handle(CreateOperationalSafetyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                safetyDomain: request.SafetyDomain,
                riskLevel: request.RiskLevel,
                ownerUserId: ownerId,
                reviewDueAtUtc: request.ReviewDueAtUtc.HasValue ? UtcTimestamp.From(request.ReviewDueAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateOperationalSafetyResult>.Success(new CreateOperationalSafetyResult(
                aggregate.OperationalSafetyId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create OperationalSafety for Tenant {TenantId}", request.TenantId);

            return Result<CreateOperationalSafetyResult>.Failure(new ApplicationError(
                "OperationalSafety.CreateFailed",
                "Unable to submit the operational-safety record at this time.",
                Guid.NewGuid()));
        }
    }
}
