// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityEventRoutingRules.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic event routing rules for a single facility:
//                  for each of the facility's required programs, which
//                  event-bus topic its events route to (ProgramEventRoutingRules),
//                  which regulator it touches, and which SOP it invokes.
//                  Computed entirely from an already-built FacilityProfile's
//                  RequiredPrograms -- no new topics, regulators, or SOPs
//                  introduced. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>
/// A single event routing rule for one of a facility's required programs: which topic its
/// events route to, which regulator it touches, and which SOP it invokes.
/// </summary>
/// <param name="Topic">The event-bus topic (<see cref="ProgramEventRoutingRules.TopicFor"/>).</param>
/// <param name="ProgramCode">The program this route belongs to (<see cref="ProgramDefinition.Code"/>).</param>
/// <param name="ProgramName">The program's canonical name.</param>
/// <param name="Regulator">The regulator this program's events touch.</param>
/// <param name="SopCode">The SOP this route invokes (<see cref="ProgramSopRequirements.For"/>).</param>
public sealed record FacilityEventRoute(string Topic, string ProgramCode, string ProgramName, Regulator Regulator, string SopCode);

/// <summary>Builds a facility's <see cref="FacilityEventRoute"/> list deterministically from its <see cref="FacilityProfile.RequiredPrograms"/>.</summary>
public static class FacilityEventRoutingRulesBuilder
{
    /// <summary>Builds one <see cref="FacilityEventRoute"/> per required program, in <see cref="FacilityProfile.RequiredPrograms"/> order.</summary>
    public static IReadOnlyList<FacilityEventRoute> Build(FacilityProfile facility) =>
        facility.RequiredPrograms
            .Select(program => new FacilityEventRoute(
                Topic: ProgramEventRoutingRules.TopicFor(program),
                ProgramCode: program.Code,
                ProgramName: program.Name,
                Regulator: program.PrimaryRegulator,
                SopCode: ProgramSopRequirements.For(program).Code))
            .ToArray();
}
