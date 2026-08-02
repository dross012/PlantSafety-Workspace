// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           CreateAuditingCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateAuditingCommand. Pure orchestration: constructs the
//                  Auditing aggregate via its domain factory method and persists it through the
//                  domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Auditing.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Auditing.Commands;

/// <summary>Handles <see cref="CreateAuditingCommand"/>.</summary>
public sealed class CreateAuditingCommandHandler : IRequestHandler<CreateAuditingCommand, Result<CreateAuditingResult>>
{
    private readonly IAuditingRepository _repository;
    private readonly ILogger<CreateAuditingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAuditingCommandHandler(IAuditingRepository repository, ILogger<CreateAuditingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAuditingResult>> Handle(CreateAuditingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var auditorId = UserId.FromGuid(request.AuditorId);

            var aggregate = PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                auditTitle: request.AuditTitle,
                auditType: request.AuditType,
                scope: request.Scope,
                auditorId: auditorId,
                scheduledAtUtc: request.ScheduledAtUtc.HasValue ? UtcTimestamp.From(request.ScheduledAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAuditingResult>.Success(new CreateAuditingResult(
                aggregate.AuditingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Auditing for Tenant {TenantId}", request.TenantId);

            return Result<CreateAuditingResult>.Failure(new ApplicationError(
                "Compliance.Auditing.CreateFailed",
                "Unable to submit the audit record at this time.",
                Guid.NewGuid()));
        }
    }
}
