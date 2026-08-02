// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           CreateRegulatoryCalendarCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateRegulatoryCalendarCommand. Pure orchestration: constructs
//                  the RegulatoryCalendar aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Handles <see cref="CreateRegulatoryCalendarCommand"/>.</summary>
public sealed class CreateRegulatoryCalendarCommandHandler : IRequestHandler<CreateRegulatoryCalendarCommand, Result<CreateRegulatoryCalendarResult>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<CreateRegulatoryCalendarCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRegulatoryCalendarCommandHandler(IRegulatoryCalendarRepository repository, ILogger<CreateRegulatoryCalendarCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateRegulatoryCalendarResult>> Handle(CreateRegulatoryCalendarCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = UserId.FromGuid(request.CreatedByUserId);
            var responsibleParty = UserId.FromGuid(request.ResponsiblePartyId);

            var aggregate = PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.Create(
                createdBy: createdBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(createdBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                eventTitle: request.EventTitle,
                regulatoryBody: request.RegulatoryBody,
                jurisdictionCode: request.JurisdictionCode,
                complianceCategory: request.ComplianceCategory,
                responsiblePartyId: responsibleParty,
                dueAtUtc: request.DueAtUtc.HasValue ? UtcTimestamp.From(request.DueAtUtc.Value) : null,
                reminderLeadDays: request.ReminderLeadDays);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateRegulatoryCalendarResult>.Success(new CreateRegulatoryCalendarResult(
                aggregate.RegulatoryCalendarId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RegulatoryCalendar for Tenant {TenantId}", request.TenantId);

            return Result<CreateRegulatoryCalendarResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.CreateFailed",
                "Unable to submit the regulatory calendar record at this time.",
                Guid.NewGuid()));
        }
    }
}
