// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           CreateManagementOfChangeCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateManagementOfChangeCommand. Pure orchestration: constructs the
//                  ManagementOfChange aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Handles <see cref="CreateManagementOfChangeCommand"/>.</summary>
public sealed class CreateManagementOfChangeCommandHandler : IRequestHandler<CreateManagementOfChangeCommand, Result<CreateManagementOfChangeResult>>
{
    private readonly IManagementOfChangeRepository _repository;
    private readonly ILogger<CreateManagementOfChangeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateManagementOfChangeCommandHandler(IManagementOfChangeRepository repository, ILogger<CreateManagementOfChangeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateManagementOfChangeResult>> Handle(CreateManagementOfChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                changeTitle: request.ChangeTitle,
                changeType: request.ChangeType,
                justification: request.Justification,
                riskLevel: request.RiskLevel,
                requestedByUserId: requesterId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateManagementOfChangeResult>.Success(new CreateManagementOfChangeResult(
                aggregate.ManagementOfChangeId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ManagementOfChange for Tenant {TenantId}", request.TenantId);

            return Result<CreateManagementOfChangeResult>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.CreateFailed",
                "Unable to submit the change request at this time.",
                Guid.NewGuid()));
        }
    }
}
