// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           CreatePolicyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreatePolicyCommand. Pure orchestration: constructs the Policy
//                  aggregate via its domain factory method and persists it through the domain
//                  repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Policy.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Policy.Commands;

/// <summary>Handles <see cref="CreatePolicyCommand"/>.</summary>
public sealed class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, Result<CreatePolicyResult>>
{
    private readonly IPolicyRepository _repository;
    private readonly ILogger<CreatePolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreatePolicyCommandHandler(IPolicyRepository repository, ILogger<CreatePolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreatePolicyResult>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                policyTitle: request.PolicyTitle,
                category: request.Category,
                ownerId: requesterId,
                acknowledgmentRequired: request.AcknowledgmentRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreatePolicyResult>.Success(new CreatePolicyResult(
                aggregate.PolicyId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Policy for Tenant {TenantId}", request.TenantId);

            return Result<CreatePolicyResult>.Failure(new ApplicationError(
                "Compliance.Policy.CreateFailed",
                "Unable to submit the policy at this time.",
                Guid.NewGuid()));
        }
    }
}
