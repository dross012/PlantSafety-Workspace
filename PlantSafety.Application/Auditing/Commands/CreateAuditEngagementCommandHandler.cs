// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateAuditEngagementCommand. Pure orchestration: constructs the AuditEngagement aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;
using PlantSafety.Domain.Auditing.SmartEnums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="CreateAuditEngagementCommand"/>.</summary>
public sealed class CreateAuditEngagementCommandHandler : IRequestHandler<CreateAuditEngagementCommand, Result<CreateAuditEngagementResult>>
{
    private readonly IAuditEngagementRepository _repository;
    private readonly ILogger<CreateAuditEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAuditEngagementCommandHandler(IAuditEngagementRepository repository, ILogger<CreateAuditEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAuditEngagementResult>> Handle(CreateAuditEngagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var discipline = AuditDisciplineType.IsValid(request.DisciplineType) ? AuditDisciplineType.From(request.DisciplineType).Code : AuditDisciplineType.Default.Code;
            var leadAuditorId = UserId.FromGuid(request.LeadAuditorId);

            var aggregate = PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                engagementTitle: request.EngagementTitle,
                disciplineType: discipline,
                scope: request.Scope,
                leadAuditorId: leadAuditorId,
                scheduledAtUtc: request.ScheduledAtUtc.HasValue ? UtcTimestamp.From(request.ScheduledAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAuditEngagementResult>.Success(new CreateAuditEngagementResult(
                aggregate.AuditEngagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AuditEngagement for Tenant {TenantId}", request.TenantId);

            return Result<CreateAuditEngagementResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.CreateFailed",
                "Unable to create the AuditEngagement record at this time.",
                Guid.NewGuid()));
        }
    }
}