// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordControlChangeEventCommand. Classifies authorization status
//   via ControlChangeAuthorizationRules, chains the tamper-evident hash from the asset's latest
//   record, and persists via the domain repository.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Rules;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;

/// <summary>Handles <see cref="RecordControlChangeEventCommand"/>.</summary>
public sealed class RecordControlChangeEventCommandHandler : IRequestHandler<RecordControlChangeEventCommand, Result<RecordControlChangeEventResult>>
{
    private readonly IControlChangeEventRepository _repository;
    private readonly ILogger<RecordControlChangeEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordControlChangeEventCommandHandler(IControlChangeEventRepository repository, ILogger<RecordControlChangeEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordControlChangeEventResult>> Handle(RecordControlChangeEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);
            var otAssetId = OtAssetId.FromGuid(request.OtAssetId);

            var changeSourceUserId = request.ChangeSourceUserId.HasValue ? UserId.FromGuid(request.ChangeSourceUserId.Value) : (UserId?)null;
            var authorizationStatus = ControlChangeAuthorizationRules.Classify(
                hasApprovedChangeRecord: request.HasApprovedChangeRecord,
                hasKnownChangeSource: changeSourceUserId.HasValue);

            var latest = await _repository.GetLatestForAssetAsync(otAssetId, request.TenantId, cancellationToken);

            var aggregate = PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates.ControlChangeEvent.Record(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                otAssetId: otAssetId,
                changeDescription: request.ChangeDescription,
                changedParameter: request.ChangedParameter,
                oldValue: request.OldValue,
                newValue: request.NewValue,
                detectedAtUtc: UtcTimestamp.From(request.DetectedAtUtc),
                priorRecordHash: latest?.RecordHash,
                changeSourceUserId: changeSourceUserId,
                authorizationStatus: authorizationStatus);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RecordControlChangeEventResult>.Success(new RecordControlChangeEventResult(
                aggregate.ControlChangeEventId.Value,
                aggregate.AuthorizationStatus.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record ControlChangeEvent for Tenant {TenantId}", request.TenantId);

            return Result<RecordControlChangeEventResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.ControlChangeEvent.RecordFailed",
                "Unable to record the control-change event at this time.",
                Guid.NewGuid()));
        }
    }
}
