// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GenerateEsgDisclosureReportCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for GenerateEsgDisclosureReportCommand. Loads the source
//                  AdvancedMetricsLedger, snapshots its current calculation methodology, and
//                  generates a new version-1 draft EsgDisclosureReport via the domain aggregate
//                  factory method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Handles <see cref="GenerateEsgDisclosureReportCommand"/>.</summary>
public sealed class GenerateEsgDisclosureReportCommandHandler : IRequestHandler<GenerateEsgDisclosureReportCommand, Result<GenerateEsgDisclosureReportResult>>
{
    private readonly IAdvancedMetricsLedgerRepository _ledgerRepository;
    private readonly IEsgDisclosureReportRepository _reportRepository;
    private readonly ILogger<GenerateEsgDisclosureReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GenerateEsgDisclosureReportCommandHandler(
        IAdvancedMetricsLedgerRepository ledgerRepository,
        IEsgDisclosureReportRepository reportRepository,
        ILogger<GenerateEsgDisclosureReportCommandHandler> logger)
    {
        _ledgerRepository = ledgerRepository;
        _reportRepository = reportRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GenerateEsgDisclosureReportResult>> Handle(GenerateEsgDisclosureReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!TryParseFrameworks(request.Frameworks, out var frameworks))
            {
                return Result<GenerateEsgDisclosureReportResult>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.InvalidDisclosureFramework",
                    $"'{request.Frameworks}' does not contain a recognized disclosure framework.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var ledgerId = AdvancedMetricsLedgerId.FromGuid(request.SourceLedgerId);
            var ledger = await _ledgerRepository.GetByIdAsync(ledgerId, request.TenantId, cancellationToken);
            if (ledger is null)
            {
                return Result<GenerateEsgDisclosureReportResult>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.SourceLedgerNotFound",
                    "The source metrics ledger for this disclosure report could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var report = PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.EsgDisclosureReport.Generate(
                generatedBy: requesterId,
                generatedAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                sourceLedgerId: ledger.AdvancedMetricsLedgerId,
                frameworks: frameworks,
                reportingPeriodStartUtc: ledger.ReportingPeriodStartUtc,
                reportingPeriodEndUtc: ledger.ReportingPeriodEndUtc,
                methodologyVersion: request.MethodologyVersion);

            await _reportRepository.AddAsync(report, cancellationToken);

            return Result<GenerateEsgDisclosureReportResult>.Success(new GenerateEsgDisclosureReportResult(
                report.EsgDisclosureReportId.Value, report.Version, report.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate ESG disclosure report for Tenant {TenantId}", request.TenantId);

            return Result<GenerateEsgDisclosureReportResult>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.GenerateEsgDisclosureReportFailed",
                "Unable to generate the ESG disclosure report at this time.",
                request.CorrelationId));
        }
    }

    private static bool TryParseFrameworks(string raw, out EsgDisclosureFramework frameworks)
    {
        frameworks = EsgDisclosureFramework.None;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        foreach (var token in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<EsgDisclosureFramework>(token, ignoreCase: true, out var parsed))
                return false;

            frameworks |= parsed;
        }

        return frameworks != EsgDisclosureFramework.None;
    }
}
