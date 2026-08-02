// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           SupersedePolicyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for SupersedePolicyCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Policy.Interfaces;
using PlantSafety.Domain.Compliance.Policy.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Policy.Commands;

/// <summary>Handles <see cref="SupersedePolicyCommand"/>.</summary>
public sealed class SupersedePolicyCommandHandler : IRequestHandler<SupersedePolicyCommand, Result<PolicyStatusResult>>
{
    private readonly IPolicyRepository _repository;
    private readonly ILogger<SupersedePolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SupersedePolicyCommandHandler(IPolicyRepository repository, ILogger<SupersedePolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PolicyStatusResult>> Handle(SupersedePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PolicyId.FromGuid(request.PolicyId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PolicyStatusResult>.Failure(new ApplicationError(
                    "Compliance.Policy.NotFound",
                    "The requested policy could not be found.",
                    request.PolicyId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Supersede(
                supersededBy: performedBy,
                supersededAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PolicyStatusResult>.Success(new PolicyStatusResult(
                aggregate.PolicyId.Value,
                aggregate.Status.ToString(),
                aggregate.AcknowledgmentCount));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to supersede Policy {PolicyId}", request.PolicyId);

            return Result<PolicyStatusResult>.Failure(new ApplicationError(
                "Compliance.Policy.SupersedeFailed",
                "Unable to supersede the policy at this time.",
                Guid.NewGuid()));
        }
    }
}
