// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           PerformedByActor.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//      Canonical ValueObject representing the actor who performed a domain
//      action. Fully compliant with the Enterprise Architectural Rule Handbook
//      (2026 Edition), specifically Section 1.8 (Actor & User Identity
//      Semantics Rule).
//
//      This ValueObject is REQUIRED for all domain actions such as:
//          - AcknowledgedBy
//          - EscalatedBy
//          - AssignedBy
//          - ResolvedBy
//          - ClosedBy
//          - ArchivedBy
//
//      CRUD authorship MUST use UserId.
//      Domain actions MUST use PerformedByActor.
//
//      This VO is SIEM-safe, audit-safe, deterministic, immutable, and
//      strong-typed. It supports:
//          - Human users
//          - System actors
//          - Workflow engines
//          - Sensors
//          - External systems
//          - Background jobs
//
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record PerformedByActor
{
    public ActorType ActorType { get; }
    public UserId? UserId { get; }
    public SystemCode? SystemCode { get; }
    public DeviceId? DeviceId { get; }

    private PerformedByActor(
        ActorType actorType,
        UserId? userId,
        SystemCode? systemCode,
        DeviceId? deviceId)
    {
        ActorType = actorType;
        UserId = userId;
        SystemCode = systemCode;
        DeviceId = deviceId;
    }

    // ------------------------------------------------------------------------
    // Factory Methods (Canonical)
    // ------------------------------------------------------------------------

    public static PerformedByActor Human(UserId userId)
        => new(ActorType.User, userId, null, null);

    public static PerformedByActor System(SystemCode systemCode)
        => new(ActorType.System, null, systemCode, null);

    public static PerformedByActor Workflow(SystemCode systemCode)
        => new(ActorType.Workflow, null, systemCode, null);

    public static PerformedByActor Sensor(DeviceId deviceId)
        => new(ActorType.Sensor, null, null, deviceId);

    public static PerformedByActor External(SystemCode systemCode)
        => new(ActorType.External, null, systemCode, null);

    // ------------------------------------------------------------------------
    // Validation
    // ------------------------------------------------------------------------

    public static PerformedByActor Create(
        ActorType actorType,
        UserId? userId,
        SystemCode? systemCode,
        DeviceId? deviceId)
    {
        actorType.EnsureValidCombination(userId, systemCode, deviceId);
        return new(actorType, userId, systemCode, deviceId);
    }

    public override string ToString()
        => ActorType.Code;
}
