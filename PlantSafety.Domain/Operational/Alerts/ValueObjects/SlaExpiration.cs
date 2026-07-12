// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           SlaExpiration.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing the expiration timestamp of an
//      SLA (Service Level Agreement) associated with an Alert. This value
//      object encapsulates a UtcTimestamp and provides semantic clarity,
//      ensuring that SLA expiration values are never passed as raw primitives.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null values (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • SlaExpiration MUST always represent a UTC timestamp (Rule 4.3).
//
//  Onboarding Notes:
//      • Used by domain logic, SLA evaluation services, and read models.
//      • SlaExpiration is NOT responsible for SLA calculations — only for
//        representing the resulting expiration moment.
//      • Validation occurs at construction time; invalid timestamps cannot
//        enter the domain.
// ============================================================================

using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record SlaExpiration(UtcTimestamp Value)
{
    public override string ToString() => Value.ToString();
}
