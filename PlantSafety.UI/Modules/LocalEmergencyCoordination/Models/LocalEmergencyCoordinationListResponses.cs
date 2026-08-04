// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side wrapper DTOs matching the wire shape returned by
//   PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Controllers.V1
//   .LocalEmergencyCoordinationV1Controller's list endpoints.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

/// <summary>Wire shape for the Mutual Aid Agreement Registry list endpoint.</summary>
public sealed class MutualAidAgreementListResponse
{
    /// <summary>Gets the matching mutual aid agreements.</summary>
    public IReadOnlyList<MutualAidAgreementDto> Items { get; init; } = [];

    /// <summary>Gets the UTC timestamp this snapshot was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>Wire shape for the Active Joint Response Status list endpoint.</summary>
public sealed class JointResponsePlanListResponse
{
    /// <summary>Gets the matching joint response plans.</summary>
    public IReadOnlyList<JointResponsePlanDto> Items { get; init; } = [];

    /// <summary>Gets the UTC timestamp this snapshot was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>Wire shape for the Post-Incident Debrief Summary list endpoint.</summary>
public sealed class JointDebriefListResponse
{
    /// <summary>Gets the matching joint debrief records.</summary>
    public IReadOnlyList<JointDebriefDto> Items { get; init; } = [];

    /// <summary>Gets the UTC timestamp this snapshot was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}
