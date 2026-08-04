// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RaiseSabotageIndicatorCommand. Persists the new indicator, then
//   runs SabotageCorrelationService against other active indicators within the correlation window
//   to apply the module's risk hook: "correlated multi-source indicators auto-escalate", and
//   "sabotage indicators feed RiskManagement at elevated priority" via SabotageRiskFeedPolicy.
//   The RiskManagement integration publish itself is an Infrastructure-layer concern (outbox
//   event consumed by a future RiskManagement integration-event handler); this handler surfaces
//   the resolved priority on the result DTO for the caller/dashboard.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Policies;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Rules;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Services;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;

/// <summary>Handles <see cref="RaiseSabotageIndicatorCommand"/>.</summary>
public sealed class RaiseSabotageIndicatorCommandHandler : IRequestHandler<RaiseSabotageIndicatorCommand, Result<RaiseSabotageIndicatorResult>>
{
    private readonly ISabotageIndicatorRepository _repository;
    private readonly ILogger<RaiseSabotageIndicatorCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RaiseSabotageIndicatorCommandHandler(ISabotageIndicatorRepository repository, ILogger<RaiseSabotageIndicatorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RaiseSabotageIndicatorResult>> Handle(RaiseSabotageIndicatorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);

            if (!Enum.TryParse<SabotageIndicatorSource>(request.Source, ignoreCase: true, out var source))
                source = SabotageIndicatorSource.Manual;

            if (!Enum.TryParse<SabotageIndicatorSeverity>(request.Severity, ignoreCase: true, out var severity))
                severity = SabotageIndicatorSeverity.Medium;

            var now = UtcTimestamp.Now();
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);

            var aggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator.Raise(
                createdBy: actorId,
                createdAtUtc: now,
                correlationId: correlationId,
                causationId: causationId,
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                source: source,
                description: request.Description,
                severity: severity,
                sourceReferenceId: request.SourceReferenceId);

            await _repository.AddAsync(aggregate, cancellationToken);

            var windowStart = now.Value.AddMinutes(-SabotageEscalationRules.CorrelationWindowMinutes);
            var candidates = await _repository.ListForCorrelationWindowAsync(request.TenantId, windowStart, now.Value, cancellationToken);
            candidates = candidates.Where(c => c.SabotageIndicatorId != aggregate.SabotageIndicatorId).ToList();

            var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(aggregate, candidates);

            if (correlatedCount > 1 || autoEscalate)
            {
                aggregate.ApplyCorrelation(correlatedCount, autoEscalate, now, PerformedByActor.System(), correlationId, causationId);
                await _repository.UpdateAsync(aggregate, cancellationToken);
            }

            var riskPriority = SabotageRiskFeedPolicy.ResolveRiskManagementPriority(aggregate.Severity);

            return Result<RaiseSabotageIndicatorResult>.Success(new RaiseSabotageIndicatorResult(
                aggregate.SabotageIndicatorId.Value,
                aggregate.Severity.ToString(),
                aggregate.Status.ToString(),
                autoEscalate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to raise SabotageIndicator for Tenant {TenantId}", request.TenantId);

            return Result<RaiseSabotageIndicatorResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.SabotageIndicator.RaiseFailed",
                "Unable to raise the sabotage indicator at this time.",
                Guid.NewGuid()));
        }
    }
}
