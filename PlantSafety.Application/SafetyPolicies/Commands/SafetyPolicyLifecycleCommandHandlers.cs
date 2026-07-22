// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handlers for the SafetyPolicy governance-lifecycle commands (submit, approve,
//   reject, publish, acknowledge, start review, complete review, retire). Pure orchestration: loads
//   the aggregate, invokes the corresponding domain method, and persists via the repository
//   interface. No business rules live here -- all invariants are enforced inside the aggregate.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.Enums;
using PlantSafety.Domain.SafetyPolicies.Interfaces;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPolicies.Commands;

/// <summary>Handles <see cref="SubmitSafetyPolicyForApprovalCommand"/>.</summary>
public sealed class SubmitSafetyPolicyForApprovalCommandHandler : IRequestHandler<SubmitSafetyPolicyForApprovalCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<SubmitSafetyPolicyForApprovalCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitSafetyPolicyForApprovalCommandHandler(ISafetyPolicyRepository repository, ILogger<SubmitSafetyPolicyForApprovalCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(SubmitSafetyPolicyForApprovalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var submittedBy = UserId.FromGuid(request.SubmittedBy);
            aggregate.SubmitForApproval(submittedBy, UtcTimestamp.Now(), PerformedByActor.User(submittedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit SafetyPolicy {SafetyPolicyId} for approval", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.SubmitFailed", "Unable to submit the policy for approval.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ApproveSafetyPolicyCommand"/>.</summary>
public sealed class ApproveSafetyPolicyCommandHandler : IRequestHandler<ApproveSafetyPolicyCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<ApproveSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<ApproveSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(ApproveSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var approvedBy = UserId.FromGuid(request.ApprovedBy);
            aggregate.Approve(approvedBy, UtcTimestamp.Now(), PerformedByActor.User(approvedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.ApproveFailed", "Unable to approve the policy.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="RejectSafetyPolicyCommand"/>.</summary>
public sealed class RejectSafetyPolicyCommandHandler : IRequestHandler<RejectSafetyPolicyCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<RejectSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RejectSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<RejectSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(RejectSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var rejectedBy = UserId.FromGuid(request.RejectedBy);
            aggregate.Reject(rejectedBy, request.Reason, UtcTimestamp.Now(), PerformedByActor.User(rejectedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.RejectFailed", "Unable to reject the policy.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="PublishSafetyPolicyCommand"/>.</summary>
public sealed class PublishSafetyPolicyCommandHandler : IRequestHandler<PublishSafetyPolicyCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<PublishSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<PublishSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(PublishSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            aggregate.Publish(UtcTimestamp.From(request.EffectiveAtUtc), PerformedByActor.System(),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.PublishFailed", "Unable to publish the policy.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="AcknowledgeSafetyPolicyCommand"/>.</summary>
public sealed class AcknowledgeSafetyPolicyCommandHandler : IRequestHandler<AcknowledgeSafetyPolicyCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<AcknowledgeSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<AcknowledgeSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(AcknowledgeSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var acknowledgedBy = UserId.FromGuid(request.AcknowledgedBy);
            aggregate.RecordAcknowledgment(acknowledgedBy, UtcTimestamp.Now(), PerformedByActor.User(acknowledgedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record acknowledgment for SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.AcknowledgeFailed", "Unable to record the acknowledgment.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="StartSafetyPolicyReviewCommand"/>.</summary>
public sealed class StartSafetyPolicyReviewCommandHandler : IRequestHandler<StartSafetyPolicyReviewCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<StartSafetyPolicyReviewCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartSafetyPolicyReviewCommandHandler(ISafetyPolicyRepository repository, ILogger<StartSafetyPolicyReviewCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(StartSafetyPolicyReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var startedBy = UserId.FromGuid(request.StartedBy);
            aggregate.StartReviewCycle(startedBy, UtcTimestamp.Now(), PerformedByActor.User(startedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start review cycle for SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.StartReviewFailed", "Unable to start the review cycle.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CompleteSafetyPolicyReviewCommand"/>.</summary>
public sealed class CompleteSafetyPolicyReviewCommandHandler : IRequestHandler<CompleteSafetyPolicyReviewCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<CompleteSafetyPolicyReviewCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteSafetyPolicyReviewCommandHandler(ISafetyPolicyRepository repository, ILogger<CompleteSafetyPolicyReviewCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(CompleteSafetyPolicyReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var reviewedBy = UserId.FromGuid(request.ReviewedBy);
            var newCadence = !string.IsNullOrWhiteSpace(request.NewReviewCadence) && ReviewCadence.IsValid(request.NewReviewCadence)
                ? ReviewCadence.From(request.NewReviewCadence)
                : null;

            aggregate.CompleteReview(reviewedBy, UtcTimestamp.Now(), newCadence, PerformedByActor.User(reviewedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete review for SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.CompleteReviewFailed", "Unable to complete the review cycle.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="RetireSafetyPolicyCommand"/>.</summary>
public sealed class RetireSafetyPolicyCommandHandler : IRequestHandler<RetireSafetyPolicyCommand, Result<SafetyPolicyStatusResult>>
{
    private readonly ISafetyPolicyRepository _repository;
    private readonly ILogger<RetireSafetyPolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireSafetyPolicyCommandHandler(ISafetyPolicyRepository repository, ILogger<RetireSafetyPolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPolicyStatusResult>> Handle(RetireSafetyPolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyPolicyId.FromGuid(request.SafetyPolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
                return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.NotFound", "SafetyPolicy not found.", request.SafetyPolicyId));

            var retiredBy = UserId.FromGuid(request.RetiredBy);
            aggregate.Retire(retiredBy, request.Reason, UtcTimestamp.Now(), PerformedByActor.User(retiredBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyPolicyStatusResult>.Success(new SafetyPolicyStatusResult(aggregate.SafetyPolicyId.Value, aggregate.Status.ToString(), aggregate.SafetyPolicyVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire SafetyPolicy {SafetyPolicyId}", request.SafetyPolicyId);
            return Result<SafetyPolicyStatusResult>.Failure(new ApplicationError("SafetyPolicies.SafetyPolicy.RetireFailed", "Unable to retire the policy.", Guid.NewGuid()));
        }
    }
}
