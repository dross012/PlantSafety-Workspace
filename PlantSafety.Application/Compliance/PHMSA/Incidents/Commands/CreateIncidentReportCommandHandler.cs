// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           CreateIncidentReportCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateIncidentReportCommand. Pure orchestration: constructs the
//                  IncidentReport aggregate via its domain factory method (which computes
//                  IsReportableToPhmsa internally) and persists it through the domain repository
//                  interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;

/// <summary>Handles <see cref="CreateIncidentReportCommand"/>.</summary>
public sealed class CreateIncidentReportCommandHandler : IRequestHandler<CreateIncidentReportCommand, Result<CreateIncidentReportResult>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<CreateIncidentReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateIncidentReportCommandHandler(IIncidentReportRepository repository, ILogger<CreateIncidentReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateIncidentReportResult>> Handle(CreateIncidentReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reportedBy = UserId.FromGuid(request.ReportedByUserId);

            if (!Enum.TryParse<IncidentType>(request.IncidentType, ignoreCase: true, out var incidentType))
            {
                return Result<CreateIncidentReportResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Incidents.InvalidIncidentType",
                    $"'{request.IncidentType}' is not a recognized incident type.",
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var aggregate = Domain.Compliance.PHMSA.Incidents.Aggregates.IncidentReport.Create(
                incidentType: incidentType,
                materialInvolvedId: request.MaterialInvolvedId.HasValue ? HazmatMaterialId.FromGuid(request.MaterialInvolvedId.Value) : null,
                materialInvolvedDescription: request.MaterialInvolvedDescription,
                injuries: request.Injuries,
                fatalitiesInvolved: request.FatalitiesInvolved,
                propertyDamageEstimateUsd: request.PropertyDamageEstimateUsd,
                narrative: request.Narrative,
                incidentDateUtc: request.IncidentDateUtc,
                location: request.Location,
                createdBy: reportedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(reportedBy));

            await _repository.AddAsync(aggregate, request.TenantId, request.SiteId, cancellationToken);

            return Result<CreateIncidentReportResult>.Success(new CreateIncidentReportResult(
                aggregate.Id.Value,
                aggregate.Status.ToString(),
                aggregate.IsReportableToPhmsa));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create IncidentReport for Tenant {TenantId}", request.TenantId);

            return Result<CreateIncidentReportResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.CreateFailed",
                "Unable to file the incident report at this time.",
                Guid.NewGuid()));
        }
    }
}
