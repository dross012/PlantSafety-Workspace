// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           DeclareCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for DeclareCrisisManagementCommand. Pure orchestration: constructs the
//                  CrisisManagement aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;

/// <summary>Handles <see cref="DeclareCrisisManagementCommand"/>.</summary>
public sealed class DeclareCrisisManagementCommandHandler : IRequestHandler<DeclareCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<DeclareCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeclareCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<DeclareCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(DeclareCrisisManagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var declaredBy = UserId.FromGuid(request.DeclaredByUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.CrisisManagement.Aggregates.CrisisManagement.Create(
                createdBy: declaredBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(declaredBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                title: request.Title,
                severity: request.Severity,
                declaredAtUtc: UtcTimestamp.From(request.DeclaredAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CrisisManagementStatusResult>.Success(new CrisisManagementStatusResult(
                aggregate.CrisisManagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare CrisisManagement crisis for Tenant {TenantId}", request.TenantId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.DeclareFailed",
                "Unable to declare the crisis at this time.",
                Guid.NewGuid()));
        }
    }
}
