// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityProfile.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable schema for a single facility's fully-computed
//                  profile: its industry binding plus every requirement
//                  derived from that binding (regulatory footprint, hazard
//                  footprint, required programs/permits/training/audits/SOPs)
//                  and its compliance scorecard. Built exclusively by
//                  FacilityProfileBuilder.Build -- never hand-constructed --
//                  since every derived field must stay consistent with the
//                  industry it was computed from.
//
//                  SCOPE NOTE: FacilityId/Name/Location are caller-supplied
//                  inputs, not owned or persisted identity. Shared.SafetyData
//                  deliberately does not define or own a persisted Facility
//                  aggregate (no database table, no repository, no
//                  create/update/delete) -- that would be a real, tenant-owned,
//                  mutable bounded-context module (the kind PlantSafety's own
//                  Domain/Application/Infrastructure/Api/UI layers build,
//                  e.g. its Tenant aggregate under PlantSafety.Domain.Tenancy),
//                  not a piece of deterministic reference/computation logic.
//                  This type is a pure computation result: given a facility's
//                  identity/industry/location and a snapshot of which
//                  requirement codes it has completed, compute everything
//                  else deterministically from the existing Industry,
//                  Regulatory, and Safety Programs subsystems. A real
//                  persisted Facility/Site entity (wherever PlantSafety
//                  eventually defines one) is expected to call
//                  FacilityProfileBuilder.Build with its own real identity
//                  and location data, not the other way around.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>
/// A single facility's fully-computed profile: its industry binding, every requirement derived
/// from that binding, and its compliance scorecard.
/// </summary>
/// <param name="FacilityId">Caller-supplied facility identifier (see this file's SCOPE NOTE -- not owned or persisted here).</param>
/// <param name="Name">Caller-supplied facility display name.</param>
/// <param name="IndustryName">The exact, canonical <see cref="IndustryDefinition.Name"/> this facility is bound to.</param>
/// <param name="Location">Caller-supplied facility location (free text).</param>
/// <param name="Flags">The bound industry's <see cref="SafetyFlag"/> bitmask (the facility's hazard footprint).</param>
/// <param name="Regulators">The bound industry's regulators (the facility's regulatory footprint).</param>
/// <param name="RequiredPrograms">Every Safety Program applicable to the bound industry (<see cref="ProgramRequirementMapping.For"/>).</param>
/// <param name="RequiredPermits">Every distinct permit type required across <paramref name="RequiredPrograms"/>.</param>
/// <param name="RequiredTraining">One training requirement per program in <paramref name="RequiredPrograms"/>.</param>
/// <param name="RequiredAudits">One audit checklist item per program in <paramref name="RequiredPrograms"/>.</param>
/// <param name="RequiredSops">One SOP template per program in <paramref name="RequiredPrograms"/>.</param>
/// <param name="ComplianceProfile">The facility's compliance scorecard (<see cref="FacilityComplianceProfile"/>).</param>
public sealed record FacilityProfile(
    Guid FacilityId,
    string Name,
    string IndustryName,
    string Location,
    SafetyFlag Flags,
    IReadOnlyList<Regulator> Regulators,
    IReadOnlyList<ProgramDefinition> RequiredPrograms,
    IReadOnlyList<PermitType> RequiredPermits,
    IReadOnlyList<TrainingRequirement> RequiredTraining,
    IReadOnlyList<AuditChecklistItem> RequiredAudits,
    IReadOnlyList<SopTemplate> RequiredSops,
    FacilityComplianceProfile ComplianceProfile);
