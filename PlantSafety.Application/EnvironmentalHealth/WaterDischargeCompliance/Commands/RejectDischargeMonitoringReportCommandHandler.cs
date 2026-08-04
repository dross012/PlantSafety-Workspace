// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RejectDischargeMonitoringReportCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="RejectDischargeMonitoringReportCommand"/>.</summary>
public sealed class RejectDischargeMonitoringReportCommandHandler : IRequestHandler<RejectDischargeMonitoringReportCommand, Result<DischargeMonitoringReportActionResult>>
{
    private readonly IDischargeMonitoringReportRepository _repository;
    private readonly ILogger<RejectDischargeMonitoringReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RejectDischargeMonitoringReportCommandHandler(IDischargeMonitoringReportRepository repository, ILogger<RejectDischargeMonitoringReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargeMonitoringReportActionResult>> Handle(RejectDischargeMonitoringReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DischargeMonitoringReportId.FromGuid(request.DischargeMonitoringReportId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<DischargeMonitoringReportActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.NotFound",
                    "The requested DischargeMonitoringReport record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Reject(
                request.Reason,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DischargeMonitoringReportActionResult>.Success(new DischargeMonitoringReportActionResult(
                aggregate.DischargeMonitoringReportId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Reject for DischargeMonitoringReport {DischargeMonitoringReportId}", request.DischargeMonitoringReportId);

            return Result<DischargeMonitoringReportActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.RejectFailed",
                "Unable to Reject the DischargeMonitoringReport record at this time.",
                Guid.NewGuid()));
        }
    }
}