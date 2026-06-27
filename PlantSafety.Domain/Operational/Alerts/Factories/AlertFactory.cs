// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertFactory.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical domain factory for constructing Alert aggregates and their
//      child entities. Applies domain validation rules, category/source rules,
//      SLA policy, and emits the AlertCreatedEvent.
//
//      Complies with:
//          - Canonical Aggregate Rule (2.5)
//          - Domain Factory Rule (2.6)
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Construction Rule (1.3)
//          - Business Model Fidelity Rule (4.4)
//          - Additive-only evolution
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Interfaces;
using PlantSafety.Domain.Operational.Alerts.Policies;
using PlantSafety.Domain.Operational.Alerts.Services;
using PlantSafety.Operational.Alerts.Domain.Alerts.Aggregates;
using PlantSafety.Operational.Alerts.Domain.Alerts.Enums;
using PlantSafety.Operational.Alerts.Domain.Alerts.Events;
using PlantSafety.Operational.Alerts.Domain.Alerts.Exceptions;
using PlantSafety.Operational.Alerts.Domain.Alerts.Interfaces;
using PlantSafety.Operational.Alerts.Domain.Alerts.Policies;
using PlantSafety.Operational.Alerts.Domain.Alerts.Rules;
using PlantSafety.Operational.Alerts.Domain.Alerts.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Identity;
using PlantSafety.SharedKernel.Lineage;
using PlantSafety.SharedKernel.Time;

namespace PlantSafety.Domain.Operational.Alerts.Factories;

public sealed class AlertFactory : IAlertFactory
{
    private readonly AlertValidationService _validationService;
    private readonly AlertSlaPolicy _slaPolicy;
    private readonly AlertCategoryPolicy _categoryPolicy;
    private readonly AlertSourcePolicy _sourcePolicy;

    public AlertFactory(
        AlertValidationService validationService,
        AlertSlaPolicy slaPolicy,
        AlertCategoryPolicy categoryPolicy,
        AlertSourcePolicy sourcePolicy)
    {
        _validationService = validationService;
        _slaPolicy = slaPolicy;
        _categoryPolicy = categoryPolicy;
        _sourcePolicy = sourcePolicy;
    }

    public Alert Create(
        AlertType type,
        AlertSeverity severity,
        string title,
        string description,
        string sourceModule,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        // ------------------------------------------------------------
        // 1. Validate core fields
        // ------------------------------------------------------------
        _validationService.ValidateForCreation(
            title,
            description,
            type,
            severity,
            sourceModule,
            correlationId,
            causationId);

        // ------------------------------------------------------------
        // 2. Validate category (if type has category)
        // ------------------------------------------------------------
        if (type.Category is not null && !_categoryPolicy.IsValidCategory(type.Category))
        {
            throw new InvalidAlertDomainException(
                $"Invalid category '{type.Category.Code}' for alert type '{type.Code}'",
                correlationId,
                causationId);
        }

        // ------------------------------------------------------------
        // 3. Validate source type (if type has source)
        // ------------------------------------------------------------
        if (type.SourceType is not null && !_sourcePolicy.IsValidSource(type.SourceType))
        {
            throw new InvalidAlertDomainException(
                $"Invalid source type '{type.SourceType.Code}' for alert type '{type.Code}'",
                correlationId,
                causationId);
        }

        // ------------------------------------------------------------
        // 4. Construct the aggregate (deterministic)
        // ------------------------------------------------------------
        var alert = new Alert
        {
            AlertId = AlertId.New(),
            AlertType = type,
            Severity = severity,
            Status = AlertStatus.Created,
            Title = title.Trim(),
            Description = description.Trim(),
            SourceModule = sourceModule.Trim(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy
        };

        // ------------------------------------------------------------
        // 5. Compute SLA expiration
        // ------------------------------------------------------------
        var slaExpiration = _slaPolicy.CalculateExpiration(alert);
        alert.SetSlaExpiration(slaExpiration);

        // ------------------------------------------------------------
        // 6. Add initial history entry
        // ------------------------------------------------------------
        alert.AddHistory(
            previous: AlertStatus.Created,
            next: AlertStatus.Created,
            changedAtUtc: createdAtUtc,
            actor: actor,
            notes: null);

        // ------------------------------------------------------------
        // 7. Emit AlertCreatedEvent
        // ------------------------------------------------------------
        var evt = new AlertCreatedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: alert.AlertId,
            AlertType: alert.AlertType,
            Severity: alert.Severity,
            SourceModule: alert.SourceModule,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        alert.AddDomainEvent(evt);

        return alert;
    }
}
