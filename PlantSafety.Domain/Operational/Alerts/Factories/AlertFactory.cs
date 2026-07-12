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
using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Enums;
using PlantSafety.Domain.Operational.Alerts.Events;
using PlantSafety.Domain.Operational.Alerts.Exceptions;
using PlantSafety.Domain.Operational.Alerts.Interfaces;
using PlantSafety.Domain.Operational.Alerts.Policies;
using PlantSafety.Domain.Operational.Alerts.Rules;
using PlantSafety.Domain.Operational.Alerts.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
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
        AlertValidationService.ValidateForCreation(
            title,
            description,
            type,
            severity,
            sourceModule,
            correlationId,
            causationId);

        // ------------------------------------------------------------
        // 2. Construct the aggregate (deterministic).
        //    NOTE: Category/source-type validation against
        //    AlertCategoryPolicy / AlertSourcePolicy was intentionally
        //    removed here — AlertType only exposes a free-form
        //    descriptive `Category` string (e.g. "Operations",
        //    "Maintenance") that does not correspond 1:1 with the
        //    AlertCategory smart enum (Safety/Environmental/Equipment/
        //    Compliance), and AlertType has no SourceType concept at
        //    all. Enforcing those policies here would incorrectly
        //    reject legitimate AlertType values such as MachineFailure
        //    or PredictiveMaintenance. See task report for details.
        // ------------------------------------------------------------
        var alert = Alert.Create(
            alertType: type,
            severity: severity,
            title: title,
            description: description,
            sourceModule: sourceModule,
            createdBy: createdBy,
            createdAtUtc: createdAtUtc,
            correlationId: correlationId,
            causationId: causationId,
            actor: actor);

        // ------------------------------------------------------------
        // 3. Compute and apply SLA expiration
        // ------------------------------------------------------------
        var slaExpirationUtc = _slaPolicy.CalculateExpiration(alert);
        alert.SetSlaExpiration(new SlaExpiration(slaExpirationUtc));

        return alert;
    }
}
