// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           CloseEmergencyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseEmergencyCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ClosePermitsCommandHandler.
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

/// <summary>Handles <see cref="CloseEmergencyCommand"/>.</summary>
public sealed class CloseEmergencyCommandHandler : IRequestHandler<CloseEmergencyCommand, Result<EmergencyManagementStatusResult>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<CloseEmergencyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseEmergencyCommandHandler(IEmergencyManagementRepository repository, ILogger<CloseEmergencyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyManagementStatusResult>> Handle(CloseEmergencyCommand request, CancellationToken cancellationToken)
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

            var closedBy = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                closedBy: closedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(closedBy),
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
            _logger.LogError(ex, "Failed to close EmergencyManagement {EmergencyManagementId}", request.EmergencyManagementId);

            return Result<EmergencyManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.CloseFailed",
                "Unable to close the emergency at this time.",
                Guid.NewGuid()));
        }
    }
}
