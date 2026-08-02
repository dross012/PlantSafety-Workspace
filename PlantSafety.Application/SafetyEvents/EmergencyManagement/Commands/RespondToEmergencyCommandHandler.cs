// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           RespondToEmergencyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RespondToEmergencyCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;

/// <summary>Handles <see cref="RespondToEmergencyCommand"/>.</summary>
public sealed class RespondToEmergencyCommandHandler : IRequestHandler<RespondToEmergencyCommand, Result<EmergencyManagementStatusResult>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<RespondToEmergencyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RespondToEmergencyCommandHandler(IEmergencyManagementRepository repository, ILogger<RespondToEmergencyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyManagementStatusResult>> Handle(RespondToEmergencyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyManagementId.FromGuid(request.EmergencyManagementId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyManagementStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.EmergencyManagement.NotFound",
                    "The requested emergency could not be found.",
                    request.EmergencyManagementId,
                    ErrorSeverity.NotFound));
            }

            var respondedBy = UserId.FromGuid(request.RespondedByUserId);

            aggregate.RespondTo(
                respondedBy: respondedBy,
                respondedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(respondedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EmergencyManagementStatusResult>.Success(new EmergencyManagementStatusResult(
                aggregate.EmergencyManagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to respond to EmergencyManagement {EmergencyManagementId}", request.EmergencyManagementId);

            return Result<EmergencyManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.RespondFailed",
                "Unable to record the emergency response at this time.",
                Guid.NewGuid()));
        }
    }
}
