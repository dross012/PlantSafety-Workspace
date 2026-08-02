// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           CreateWorkerWellBeingCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateWorkerWellBeingCommand. Pure orchestration: constructs the
//                  WorkerWellBeing aggregate via its domain factory method and persists it
//                  through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.WorkerWellBeing.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.WorkerWellBeing.Commands;

/// <summary>Handles <see cref="CreateWorkerWellBeingCommand"/>.</summary>
public sealed class CreateWorkerWellBeingCommandHandler : IRequestHandler<CreateWorkerWellBeingCommand, Result<CreateWorkerWellBeingResult>>
{
    private readonly IWorkerWellBeingRepository _repository;
    private readonly ILogger<CreateWorkerWellBeingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateWorkerWellBeingCommandHandler(IWorkerWellBeingRepository repository, ILogger<CreateWorkerWellBeingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateWorkerWellBeingResult>> Handle(CreateWorkerWellBeingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var workerUserId = UserId.FromGuid(request.WorkerUserId);

            var aggregate = PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                workerId: workerUserId,
                wellBeingRiskLevel: request.WellBeingRiskLevel,
                checkInType: request.CheckInType,
                wellBeingScore: request.WellBeingScore,
                followUpRequired: request.FollowUpRequired,
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateWorkerWellBeingResult>.Success(new CreateWorkerWellBeingResult(
                aggregate.WorkerWellBeingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create WorkerWellBeing record for Tenant {TenantId}", request.TenantId);

            return Result<CreateWorkerWellBeingResult>.Failure(new ApplicationError(
                "WorkerWellBeing.WorkerWellBeing.CreateFailed",
                "Unable to submit the worker wellbeing check-in record at this time.",
                Guid.NewGuid()));
        }
    }
}
