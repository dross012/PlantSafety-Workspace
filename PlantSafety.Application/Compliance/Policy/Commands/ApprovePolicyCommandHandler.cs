// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           ApprovePolicyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ApprovePolicyCommand. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException is
//                  re-thrown before the generic catch -- all from day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
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

/// <summary>Handles <see cref="ApprovePolicyCommand"/>.</summary>
public sealed class ApprovePolicyCommandHandler : IRequestHandler<ApprovePolicyCommand, Result<PolicyStatusResult>>
{
    private readonly IPolicyRepository _repository;
    private readonly ILogger<ApprovePolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApprovePolicyCommandHandler(IPolicyRepository repository, ILogger<ApprovePolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PolicyStatusResult>> Handle(ApprovePolicyCommand request, CancellationToken cancellationToken)
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

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                effectiveAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
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
            _logger.LogError(ex, "Failed to approve Policy {PolicyId}", request.PolicyId);

            return Result<PolicyStatusResult>.Failure(new ApplicationError(
                "Compliance.Policy.ApproveFailed",
                "Unable to approve the policy at this time.",
                Guid.NewGuid()));
        }
    }
}
