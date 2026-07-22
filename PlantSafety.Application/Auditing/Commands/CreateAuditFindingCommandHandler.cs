// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateAuditFindingCommand. Pure orchestration: constructs the AuditFinding aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditFinding.Interfaces;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="CreateAuditFindingCommand"/>.</summary>
public sealed class CreateAuditFindingCommandHandler : IRequestHandler<CreateAuditFindingCommand, Result<CreateAuditFindingResult>>
{
    private readonly IAuditFindingRepository _repository;
    private readonly ILogger<CreateAuditFindingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAuditFindingCommandHandler(IAuditFindingRepository repository, ILogger<CreateAuditFindingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAuditFindingResult>> Handle(CreateAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var assignedToUserId = request.AssignedToUserId.HasValue ? UserId.FromGuid(request.AssignedToUserId.Value) : (UserId?)null;

            var aggregate = PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                auditEngagementId: request.AuditEngagementId,
                findingTitle: request.FindingTitle,
                severity: request.Severity,
                correctiveActionRequired: request.CorrectiveActionRequired,
                assignedToUserId: assignedToUserId,
                dueDateUtc: request.DueDateUtc.HasValue ? UtcTimestamp.From(request.DueDateUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAuditFindingResult>.Success(new CreateAuditFindingResult(
                aggregate.AuditFindingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AuditFinding for Tenant {TenantId}", request.TenantId);

            return Result<CreateAuditFindingResult>.Failure(new ApplicationError(
                "Auditing.AuditFinding.CreateFailed",
                "Unable to create the AuditFinding record at this time.",
                Guid.NewGuid()));
        }
    }
}