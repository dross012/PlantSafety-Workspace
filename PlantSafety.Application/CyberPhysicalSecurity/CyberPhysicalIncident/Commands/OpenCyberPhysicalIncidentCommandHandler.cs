// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for OpenCyberPhysicalIncidentCommand. Pure orchestration: constructs
//   the CyberPhysicalIncident aggregate via its domain factory method and persists it.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;

/// <summary>Handles <see cref="OpenCyberPhysicalIncidentCommand"/>.</summary>
public sealed class OpenCyberPhysicalIncidentCommandHandler : IRequestHandler<OpenCyberPhysicalIncidentCommand, Result<OpenCyberPhysicalIncidentResult>>
{
    private readonly ICyberPhysicalIncidentRepository _repository;
    private readonly ILogger<OpenCyberPhysicalIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public OpenCyberPhysicalIncidentCommandHandler(ICyberPhysicalIncidentRepository repository, ILogger<OpenCyberPhysicalIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OpenCyberPhysicalIncidentResult>> Handle(OpenCyberPhysicalIncidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);
            var ownerId = UserId.FromGuid(request.OwnerId);

            if (!Enum.TryParse<CyberPhysicalIncidentSeverity>(request.Severity, ignoreCase: true, out var severity))
                severity = CyberPhysicalIncidentSeverity.Medium;

            var aggregate = PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates.CyberPhysicalIncident.Open(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                title: request.Title,
                description: request.Description,
                severity: severity,
                ownerId: ownerId,
                relatedSabotageIndicatorIds: request.RelatedSabotageIndicatorIds);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<OpenCyberPhysicalIncidentResult>.Success(new OpenCyberPhysicalIncidentResult(
                aggregate.CyberPhysicalIncidentId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open CyberPhysicalIncident for Tenant {TenantId}", request.TenantId);

            return Result<OpenCyberPhysicalIncidentResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.CyberPhysicalIncident.OpenFailed",
                "Unable to open the cyber-physical incident at this time.",
                Guid.NewGuid()));
        }
    }
}
