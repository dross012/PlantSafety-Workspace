// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           CreateComplianceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateComplianceCommand. Pure orchestration: constructs the
//                  (self-named) Compliance aggregate via its domain factory method and persists it
//                  through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  The aggregate type is referenced via the "ComplianceAggregate" alias throughout
//                  this file -- the bare simple name "Compliance" would collide with this file's
//                  own namespace segment (PlantSafety.Application.Compliance.Compliance).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Compliance.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using ComplianceAggregate = PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance;

namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Handles <see cref="CreateComplianceCommand"/>.</summary>
public sealed class CreateComplianceCommandHandler : IRequestHandler<CreateComplianceCommand, Result<CreateComplianceResult>>
{
    private readonly IComplianceRepository _repository;
    private readonly ILogger<CreateComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateComplianceCommandHandler(IComplianceRepository repository, ILogger<CreateComplianceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateComplianceResult>> Handle(CreateComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var responsiblePartyId = UserId.FromGuid(request.ResponsiblePartyId);

            var aggregate = ComplianceAggregate.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                regulationReference: request.RegulationReference,
                ruleDescription: request.RuleDescription,
                complianceScore: request.ComplianceScore,
                nonConformanceCount: request.NonConformanceCount,
                responsiblePartyId: responsiblePartyId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateComplianceResult>.Success(new CreateComplianceResult(
                aggregate.ComplianceId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Compliance record for Tenant {TenantId}", request.TenantId);

            return Result<CreateComplianceResult>.Failure(new ApplicationError(
                "Compliance.Compliance.CreateFailed",
                "Unable to submit the compliance record at this time.",
                Guid.NewGuid()));
        }
    }
}
