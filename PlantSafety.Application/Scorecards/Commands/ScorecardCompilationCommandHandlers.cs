// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handlers for the real Scorecard compilation/distribution workflow commands.
//   Pure orchestration: loads the aggregate, invokes the corresponding domain method, and
//   persists via the repository interface. No business rules live here -- KPI formula computation
//   lives in KpiMetricDefinition and lifecycle invariants live in the Scorecard aggregate.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.Interfaces;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scorecards.Commands;

/// <summary>Handles <see cref="RecordScorecardMetricCommand"/>.</summary>
public sealed class RecordScorecardMetricCommandHandler : IRequestHandler<RecordScorecardMetricCommand, Result<RecordScorecardMetricResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<RecordScorecardMetricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordScorecardMetricCommandHandler(IScorecardRepository repository, ILogger<RecordScorecardMetricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordScorecardMetricResult>> Handle(RecordScorecardMetricCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(ScorecardId.FromGuid(request.ScorecardId), cancellationToken);
            if (aggregate is null)
                return Result<RecordScorecardMetricResult>.Failure(new ApplicationError("Scorecards.Scorecard.NotFound", "Scorecard not found.", request.ScorecardId));

            var recordedBy = UserId.FromGuid(request.RecordedBy);
            var metric = KpiMetricDefinition.IsValid(request.Metric) ? KpiMetricDefinition.From(request.Metric) : KpiMetricDefinition.Default;

            var measurement = aggregate.RecordMetric(metric, request.Numerator, request.Denominator, request.DataSourceModule,
                UtcTimestamp.Now(), PerformedByActor.User(recordedBy), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<RecordScorecardMetricResult>.Success(new RecordScorecardMetricResult(
                aggregate.ScorecardId.Value, measurement.KpiMeasurementId.Value, measurement.ComputedValue, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record metric for Scorecard {ScorecardId}", request.ScorecardId);
            return Result<RecordScorecardMetricResult>.Failure(new ApplicationError("Scorecards.Scorecard.RecordMetricFailed", "Unable to record the KPI measurement.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CompleteScorecardCompilationCommand"/>.</summary>
public sealed class CompleteScorecardCompilationCommandHandler : IRequestHandler<CompleteScorecardCompilationCommand, Result<ScorecardStatusResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<CompleteScorecardCompilationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteScorecardCompilationCommandHandler(IScorecardRepository repository, ILogger<CompleteScorecardCompilationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardStatusResult>> Handle(CompleteScorecardCompilationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(ScorecardId.FromGuid(request.ScorecardId), cancellationToken);
            if (aggregate is null)
                return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.NotFound", "Scorecard not found.", request.ScorecardId));

            var completedBy = UserId.FromGuid(request.CompletedBy);
            aggregate.CompleteCompilation(UtcTimestamp.Now(), PerformedByActor.User(completedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<ScorecardStatusResult>.Success(new ScorecardStatusResult(aggregate.ScorecardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete compilation for Scorecard {ScorecardId}", request.ScorecardId);
            return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.CompleteCompilationFailed", "Unable to complete compilation.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="PublishScorecardCommand"/>.</summary>
public sealed class PublishScorecardCommandHandler : IRequestHandler<PublishScorecardCommand, Result<ScorecardStatusResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<PublishScorecardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishScorecardCommandHandler(IScorecardRepository repository, ILogger<PublishScorecardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardStatusResult>> Handle(PublishScorecardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(ScorecardId.FromGuid(request.ScorecardId), cancellationToken);
            if (aggregate is null)
                return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.NotFound", "Scorecard not found.", request.ScorecardId));

            var publishedBy = UserId.FromGuid(request.PublishedBy);
            aggregate.Publish(UtcTimestamp.Now(), PerformedByActor.User(publishedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<ScorecardStatusResult>.Success(new ScorecardStatusResult(aggregate.ScorecardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Scorecard {ScorecardId}", request.ScorecardId);
            return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.PublishFailed", "Unable to publish the scorecard.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="RecordScorecardDistributionCommand"/>.</summary>
public sealed class RecordScorecardDistributionCommandHandler : IRequestHandler<RecordScorecardDistributionCommand, Result<ScorecardStatusResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<RecordScorecardDistributionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordScorecardDistributionCommandHandler(IScorecardRepository repository, ILogger<RecordScorecardDistributionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardStatusResult>> Handle(RecordScorecardDistributionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(ScorecardId.FromGuid(request.ScorecardId), cancellationToken);
            if (aggregate is null)
                return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.NotFound", "Scorecard not found.", request.ScorecardId));

            var distributedBy = UserId.FromGuid(request.DistributedBy);
            aggregate.RecordDistribution(request.Channel, UtcTimestamp.Now(), PerformedByActor.User(distributedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<ScorecardStatusResult>.Success(new ScorecardStatusResult(aggregate.ScorecardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record distribution for Scorecard {ScorecardId}", request.ScorecardId);
            return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.RecordDistributionFailed", "Unable to record the distribution event.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CloseScorecardCommand"/>.</summary>
public sealed class CloseScorecardCommandHandler : IRequestHandler<CloseScorecardCommand, Result<ScorecardStatusResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<CloseScorecardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseScorecardCommandHandler(IScorecardRepository repository, ILogger<CloseScorecardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardStatusResult>> Handle(CloseScorecardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(ScorecardId.FromGuid(request.ScorecardId), cancellationToken);
            if (aggregate is null)
                return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.NotFound", "Scorecard not found.", request.ScorecardId));

            var closedBy = UserId.FromGuid(request.ClosedBy);
            aggregate.Close(UtcTimestamp.Now(), PerformedByActor.User(closedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<ScorecardStatusResult>.Success(new ScorecardStatusResult(aggregate.ScorecardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Scorecard {ScorecardId}", request.ScorecardId);
            return Result<ScorecardStatusResult>.Failure(new ApplicationError("Scorecards.Scorecard.CloseFailed", "Unable to close the scorecard.", Guid.NewGuid()));
        }
    }
}
