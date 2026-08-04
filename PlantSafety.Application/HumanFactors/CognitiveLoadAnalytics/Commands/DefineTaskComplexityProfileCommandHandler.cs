// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           DefineTaskComplexityProfileCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for DefineTaskComplexityProfileCommand. Get-or-creates the open
//                  CognitiveLoadLedger for the tenant/site/shift, then defines the profile on it
//                  via the domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Handles <see cref="DefineTaskComplexityProfileCommand"/>.</summary>
public sealed class DefineTaskComplexityProfileCommandHandler : IRequestHandler<DefineTaskComplexityProfileCommand, Result<DefineTaskComplexityProfileResult>>
{
    private readonly ICognitiveLoadLedgerRepository _repository;
    private readonly ILogger<DefineTaskComplexityProfileCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineTaskComplexityProfileCommandHandler(ICognitiveLoadLedgerRepository repository, ILogger<DefineTaskComplexityProfileCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DefineTaskComplexityProfileResult>> Handle(DefineTaskComplexityProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<ShiftType>(request.ShiftType, ignoreCase: true, out var shiftType))
            {
                return Result<DefineTaskComplexityProfileResult>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.InvalidShiftType",
                    $"'{request.ShiftType}' is not a recognized shift type.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var shiftDateUtc = UtcTimestamp.From(request.ShiftDateUtc);

            var ledger = await _repository.GetOpenLedgerAsync(request.TenantId, request.SiteId, shiftType, shiftDateUtc, cancellationToken);
            var isNewLedger = ledger is null;
            ledger ??= PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates.CognitiveLoadLedger.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                shiftType: shiftType,
                shiftDateUtc: shiftDateUtc);

            var profile = ledger.DefineTaskComplexityProfile(
                taskCode: request.TaskCode,
                taskName: request.TaskName,
                complexityScore: request.ComplexityScore,
                description: request.Description,
                definedAtUtc: UtcTimestamp.Now(),
                definedBy: requesterId,
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<DefineTaskComplexityProfileResult>.Success(new DefineTaskComplexityProfileResult(
                ledger.CognitiveLoadLedgerId.Value, profile.TaskComplexityProfileId.Value, profile.TaskCode, profile.ComplexityScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define task complexity profile for Tenant {TenantId}", request.TenantId);

            return Result<DefineTaskComplexityProfileResult>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.DefineTaskComplexityProfileFailed",
                "Unable to define the task complexity profile at this time.",
                request.CorrelationId));
        }
    }
}
