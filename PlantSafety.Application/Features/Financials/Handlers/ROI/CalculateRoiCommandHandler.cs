// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for CalculateRoiCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Financials.Commands.ROI.CalculateROI;

namespace PlantSafety.Application.Features.Financials.Handlers.ROI;

/// <summary>Handles <see cref="CalculateRoiCommand"/>. Returns the new RoiCalculationId.</summary>
public sealed class CalculateRoiCommandHandler : ICommandHandler<CalculateRoiCommand, Guid>
{
    private readonly IRoiRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CalculateRoiCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CalculateRoiCommandHandler(
        IRoiRepository repository,
        IDateTimeProvider clock,
        ILogger<CalculateRoiCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CalculateRoiCommand request, CancellationToken cancellationToken)
    {
        var roiCalculationId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        var netBenefit = request.EstimatedAvoidedCosts - request.TotalInvestment;
        var roiPercentage = request.TotalInvestment == 0m
            ? 0m
            : Math.Round(netBenefit / request.TotalInvestment * 100m, 2);

        await _repository.CreateAsync(
            roiCalculationId: roiCalculationId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            fiscalYear: request.FiscalYear,
            totalInvestment: request.TotalInvestment,
            estimatedAvoidedCosts: request.EstimatedAvoidedCosts,
            netBenefit: netBenefit,
            roiPercentage: roiPercentage,
            currencyCode: request.CurrencyCode,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "ROI {RoiCalculationId} calculated FiscalYear {FiscalYear} for Tenant {TenantId} by Actor {ActorId}",
            roiCalculationId, request.FiscalYear, request.TenantId, request.ActorId);

        return roiCalculationId;
    }
}
