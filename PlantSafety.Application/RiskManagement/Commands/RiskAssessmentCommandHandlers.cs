// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handlers for the real 3-phase Risk Management workflow commands. Pure
//   orchestration: loads the aggregate, invokes the corresponding domain method, and persists via
//   the repository interface. No business rules live here -- all invariants (score ranges, control
//   justification requirements, residual-risk acceptability) are enforced inside the Risk
//   aggregate and its RiskScore value object.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- Two Phase 4 fixes: (1) GetByIdAsync now scoped by request.TenantId,
//   closing a cross-tenant read/mutate gap (usp_Risk_GetById previously filtered by RiskId only);
//   (2) NotFound ApplicationErrors now pass ErrorSeverity.NotFound explicitly -- the default
//   severity is Error (->500), so a missing Risk previously returned 500, not 404.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.RiskManagement.Commands;

/// <summary>Handles <see cref="ScoreRiskCommand"/>.</summary>
public sealed class ScoreRiskCommandHandler : IRequestHandler<ScoreRiskCommand, Result<RiskStatusResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<ScoreRiskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScoreRiskCommandHandler(IRiskRepository repository, ILogger<ScoreRiskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskStatusResult>> Handle(ScoreRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var scoredBy = UserId.FromGuid(request.ScoredBy);
            aggregate.ScoreRisk(request.Severity, request.Likelihood, UtcTimestamp.Now(), PerformedByActor.User(scoredBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<RiskStatusResult>.Success(new RiskStatusResult(aggregate.RiskId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to score Risk {RiskId}", request.RiskId);
            return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.ScoreFailed", "Unable to score the risk.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="PrioritizeRiskCommand"/>.</summary>
public sealed class PrioritizeRiskCommandHandler : IRequestHandler<PrioritizeRiskCommand, Result<RiskStatusResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<PrioritizeRiskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PrioritizeRiskCommandHandler(IRiskRepository repository, ILogger<PrioritizeRiskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskStatusResult>> Handle(PrioritizeRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var prioritizedBy = UserId.FromGuid(request.PrioritizedBy);
            var priority = !string.IsNullOrWhiteSpace(request.Priority) && RiskPriority.IsValid(request.Priority)
                ? RiskPriority.From(request.Priority)
                : null;

            aggregate.Prioritize(priority, UtcTimestamp.Now(), PerformedByActor.User(prioritizedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<RiskStatusResult>.Success(new RiskStatusResult(aggregate.RiskId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prioritize Risk {RiskId}", request.RiskId);
            return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.PrioritizeFailed", "Unable to prioritize the risk.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ApplyRiskControlMeasureCommand"/>.</summary>
public sealed class ApplyRiskControlMeasureCommandHandler : IRequestHandler<ApplyRiskControlMeasureCommand, Result<ApplyRiskControlMeasureResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<ApplyRiskControlMeasureCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApplyRiskControlMeasureCommandHandler(IRiskRepository repository, ILogger<ApplyRiskControlMeasureCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ApplyRiskControlMeasureResult>> Handle(ApplyRiskControlMeasureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<ApplyRiskControlMeasureResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var responsiblePersonId = UserId.FromGuid(request.ResponsiblePersonId);
            var hierarchyLevel = ControlHierarchyLevel.IsValid(request.HierarchyLevel) ? ControlHierarchyLevel.From(request.HierarchyLevel) : ControlHierarchyLevel.Default;

            var measure = aggregate.ApplyControlMeasure(hierarchyLevel, request.Description, request.Justification, responsiblePersonId,
                UtcTimestamp.Now(), PerformedByActor.User(responsiblePersonId), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<ApplyRiskControlMeasureResult>.Success(new ApplyRiskControlMeasureResult(aggregate.RiskId.Value, measure.ControlMeasureId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply control measure to Risk {RiskId}", request.RiskId);
            return Result<ApplyRiskControlMeasureResult>.Failure(new ApplicationError("RiskManagement.Risk.ApplyControlMeasureFailed", "Unable to apply the control measure.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ValidateRiskEffectivenessCommand"/>.</summary>
public sealed class ValidateRiskEffectivenessCommandHandler : IRequestHandler<ValidateRiskEffectivenessCommand, Result<ValidateRiskEffectivenessResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<ValidateRiskEffectivenessCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ValidateRiskEffectivenessCommandHandler(IRiskRepository repository, ILogger<ValidateRiskEffectivenessCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ValidateRiskEffectivenessResult>> Handle(ValidateRiskEffectivenessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<ValidateRiskEffectivenessResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var validatedBy = UserId.FromGuid(request.ValidatedBy);
            var isAcceptable = aggregate.ValidateEffectiveness(request.ResidualSeverity, request.ResidualLikelihood, UtcTimestamp.Now(),
                PerformedByActor.User(validatedBy), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<ValidateRiskEffectivenessResult>.Success(new ValidateRiskEffectivenessResult(
                aggregate.RiskId.Value, aggregate.Status.ToString(), isAcceptable, aggregate.ResidualScore?.Score ?? 0));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate effectiveness for Risk {RiskId}", request.RiskId);
            return Result<ValidateRiskEffectivenessResult>.Failure(new ApplicationError("RiskManagement.Risk.ValidateEffectivenessFailed", "Unable to validate risk effectiveness.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="StartRiskReviewCycleCommand"/>.</summary>
public sealed class StartRiskReviewCycleCommandHandler : IRequestHandler<StartRiskReviewCycleCommand, Result<RiskStatusResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<StartRiskReviewCycleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartRiskReviewCycleCommandHandler(IRiskRepository repository, ILogger<StartRiskReviewCycleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskStatusResult>> Handle(StartRiskReviewCycleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var startedBy = UserId.FromGuid(request.StartedBy);
            aggregate.StartReviewCycle(UtcTimestamp.Now(), PerformedByActor.User(startedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<RiskStatusResult>.Success(new RiskStatusResult(aggregate.RiskId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start review cycle for Risk {RiskId}", request.RiskId);
            return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.StartReviewFailed", "Unable to start the review cycle.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CompleteRiskReviewCommand"/>.</summary>
public sealed class CompleteRiskReviewCommandHandler : IRequestHandler<CompleteRiskReviewCommand, Result<RiskStatusResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<CompleteRiskReviewCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteRiskReviewCommandHandler(IRiskRepository repository, ILogger<CompleteRiskReviewCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskStatusResult>> Handle(CompleteRiskReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var reviewedBy = UserId.FromGuid(request.ReviewedBy);
            var newCadence = !string.IsNullOrWhiteSpace(request.NewReviewCadence) && RiskReviewCadence.IsValid(request.NewReviewCadence)
                ? RiskReviewCadence.From(request.NewReviewCadence)
                : null;

            aggregate.CompleteReview(newCadence, UtcTimestamp.Now(), PerformedByActor.User(reviewedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<RiskStatusResult>.Success(new RiskStatusResult(aggregate.RiskId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete review for Risk {RiskId}", request.RiskId);
            return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.CompleteReviewFailed", "Unable to complete the review cycle.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CloseRiskCommand"/>.</summary>
public sealed class CloseRiskCommandHandler : IRequestHandler<CloseRiskCommand, Result<RiskStatusResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<CloseRiskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseRiskCommandHandler(IRiskRepository repository, ILogger<CloseRiskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskStatusResult>> Handle(CloseRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(RiskId.FromGuid(request.RiskId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.NotFound", "Risk not found.", request.RiskId, ErrorSeverity.NotFound));

            var closedBy = UserId.FromGuid(request.ClosedBy);
            aggregate.Close(closedBy, UtcTimestamp.Now(), PerformedByActor.User(closedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);
            return Result<RiskStatusResult>.Success(new RiskStatusResult(aggregate.RiskId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Risk {RiskId}", request.RiskId);
            return Result<RiskStatusResult>.Failure(new ApplicationError("RiskManagement.Risk.CloseFailed", "Unable to close the risk.", Guid.NewGuid()));
        }
    }
}
