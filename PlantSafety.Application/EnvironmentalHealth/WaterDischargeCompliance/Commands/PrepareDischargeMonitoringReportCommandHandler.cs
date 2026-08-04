// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for PrepareDischargeMonitoringReportCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="PrepareDischargeMonitoringReportCommand"/>.</summary>
public sealed class PrepareDischargeMonitoringReportCommandHandler : IRequestHandler<PrepareDischargeMonitoringReportCommand, Result<DischargeMonitoringReportActionResult>>
{
    private readonly IDischargeMonitoringReportRepository _repository;
    private readonly ILogger<PrepareDischargeMonitoringReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PrepareDischargeMonitoringReportCommandHandler(IDischargeMonitoringReportRepository repository, ILogger<PrepareDischargeMonitoringReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargeMonitoringReportActionResult>> Handle(PrepareDischargeMonitoringReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dischargePermitId: request.DischargePermitId,
                reportingPeriodStartUtc: UtcTimestamp.From(request.ReportingPeriodStartUtc),
                reportingPeriodEndUtc: UtcTimestamp.From(request.ReportingPeriodEndUtc),
                dueAtUtc: UtcTimestamp.From(request.DueAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DischargeMonitoringReportActionResult>.Success(new DischargeMonitoringReportActionResult(
                aggregate.DischargeMonitoringReportId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare DischargeMonitoringReport for tenant {TenantId}", request.TenantId);

            return Result<DischargeMonitoringReportActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.PrepareFailed",
                "Unable to prepare the discharge monitoring report at this time.",
                Guid.NewGuid()));
        }
    }
}