// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for IssueDischargePermitCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="IssueDischargePermitCommand"/>.</summary>
public sealed class IssueDischargePermitCommandHandler : IRequestHandler<IssueDischargePermitCommand, Result<DischargePermitActionResult>>
{
    private readonly IDischargePermitRepository _repository;
    private readonly ILogger<IssueDischargePermitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public IssueDischargePermitCommandHandler(IDischargePermitRepository repository, ILogger<IssueDischargePermitCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargePermitActionResult>> Handle(IssueDischargePermitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var program = Enum.TryParse<RegulatoryProgramType>(request.RegulatoryProgram, true, out var parsedProgram) ? parsedProgram : RegulatoryProgramType.Other;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                permitNumber: request.PermitNumber,
                regulatoryProgram: program,
                issuingAuthority: request.IssuingAuthority,
                effectiveAtUtc: UtcTimestamp.From(request.EffectiveAtUtc),
                expiresAtUtc: UtcTimestamp.From(request.ExpiresAtUtc),
                permitConditionSummary: request.PermitConditionSummary);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DischargePermitActionResult>.Success(new DischargePermitActionResult(
                aggregate.DischargePermitId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to issue DischargePermit for tenant {TenantId}", request.TenantId);

            return Result<DischargePermitActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.IssueFailed",
                "Unable to issue the discharge permit at this time.",
                Guid.NewGuid()));
        }
    }
}