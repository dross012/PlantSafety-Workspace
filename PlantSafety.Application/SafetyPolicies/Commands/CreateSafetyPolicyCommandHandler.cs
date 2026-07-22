// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateSafetyPolicyCommand. Pure orchestration: constructs the SafetyPolicy aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.Enums;
using PlantSafety.Domain.SafetyPolicies.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPolicies.Commands;

/// <summary>Handles <see cref="CreateSafetyPolicyCommand"/>.</summary>
public sealed class CreateSafetyPolicyCommandHandler : IRequestHandler<CreateSafetyPolicyCommand, Result<CreateSafetyPolicyResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<CreateSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<CreateSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateSafetyPolicyResult>> Handle(CreateSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var category = PolicyCategory.IsValid(request.Category) ? PolicyCategory.From(request.Category) : PolicyCategory.Default;

            var aggregate = PlantSafety.Domain.SafetyPolicies.Aggregates.SafetyPolicy.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                safetyPolicyTitle: request.SafetyPolicyTitle,
                category: category,
                ownerId: ownerId,
                reviewRequired: request.ReviewRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateSafetyPolicyResult>.Success(new CreateSafetyPolicyResult(
                aggregate.SafetyPolicyId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SafetyPolicy for Tenant {TenantId}", request.TenantId);

            return Result<CreateSafetyPolicyResult>.Failure(new ApplicationError(
                "SafetyPolicies.SafetyPolicy.CreateFailed",
                "Unable to create the safetypolicy record at this time.",
                Guid.NewGuid()));
        }
    }
}