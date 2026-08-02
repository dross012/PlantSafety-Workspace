// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           ResolveEmergencyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ResolveEmergencyCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CompletePermitsCommandHandler.
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

/// <summary>Handles <see cref="ResolveEmergencyCommand"/>.</summary>
public sealed class ResolveEmergencyCommandHandler : IRequestHandler<ResolveEmergencyCommand, Result<EmergencyManagementStatusResult>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<ResolveEmergencyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveEmergencyCommandHandler(IEmergencyManagementRepository repository, ILogger<ResolveEmergencyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyManagementStatusResult>> Handle(ResolveEmergencyCommand request, CancellationToken cancellationToken)
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

            var resolvedBy = UserId.FromGuid(request.ResolvedByUserId);

            aggregate.Resolve(
                resolvedBy: resolvedBy,
                resolvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(resolvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EmergencyManagementStatusResult>.Success(new EmergencyManagementStatusResult(
                aggregate.EmergencyManagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve EmergencyManagement {EmergencyManagementId}", request.EmergencyManagementId);

            return Result<EmergencyManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.ResolveFailed",
                "Unable to resolve the emergency at this time.",
                Guid.NewGuid()));
        }
    }
}
