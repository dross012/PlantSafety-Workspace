// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           CreateGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateGovernanceCommand. Pure orchestration: constructs the
//                  Governance aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Governance.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Governance.Commands;

/// <summary>Handles <see cref="CreateGovernanceCommand"/>.</summary>
public sealed class CreateGovernanceCommandHandler : IRequestHandler<CreateGovernanceCommand, Result<CreateGovernanceResult>>
{
    private readonly IGovernanceRepository _repository;
    private readonly ILogger<CreateGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateGovernanceCommandHandler(IGovernanceRepository repository, ILogger<CreateGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateGovernanceResult>> Handle(CreateGovernanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.Governance.Aggregates.Governance.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                governanceArea: request.GovernanceArea,
                policyReferenceId: request.PolicyReferenceId,
                reviewCycleMonths: request.ReviewCycleMonths,
                ownerUserId: ownerId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateGovernanceResult>.Success(new CreateGovernanceResult(
                aggregate.GovernanceId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Governance for Tenant {TenantId}", request.TenantId);

            return Result<CreateGovernanceResult>.Failure(new ApplicationError(
                "Governance.CreateFailed",
                "Unable to submit the governance record at this time.",
                Guid.NewGuid()));
        }
    }
}
