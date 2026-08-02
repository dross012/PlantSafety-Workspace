// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           CreatePermitsCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for CreatePermitsCommand. Pure orchestration: constructs the Permits
//                  aggregate via its domain factory method and persists it through the domain
//                  repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.PermitToWork.Commands.RequestPermitWorkflowCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Permits.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Handles <see cref="CreatePermitsCommand"/>.</summary>
public sealed class CreatePermitsCommandHandler : IRequestHandler<CreatePermitsCommand, Result<CreatePermitsResult>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<CreatePermitsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreatePermitsCommandHandler(IPermitsRepository repository, ILogger<CreatePermitsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreatePermitsResult>> Handle(CreatePermitsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                permitType: request.PermitType,
                permitNumber: request.PermitNumber,
                workDescription: request.WorkDescription,
                location: request.Location,
                hazardsIdentified: request.HazardsIdentified,
                requestedByUserId: requesterId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreatePermitsResult>.Success(new CreatePermitsResult(
                aggregate.PermitsId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Permits for Tenant {TenantId}", request.TenantId);

            return Result<CreatePermitsResult>.Failure(new ApplicationError(
                "Compliance.Permits.CreateFailed",
                "Unable to submit the permit request at this time.",
                Guid.NewGuid()));
        }
    }
}
