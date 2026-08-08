// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramEventRoutingRules.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Program -> Event routing topic mapping.
//                  When the program has a PrimaryFlag, its topic is the
//                  existing EventRoutingTopicCatalog.TopicForFlag value --
//                  identical to what the flag's own hazard events already
//                  route to, so a program-level subscriber and a
//                  flag-level subscriber (EventRoutingRules.cs) observe the
//                  same stream for that hazard. When PrimaryFlag is null
//                  (FDA's quality/sanitation programs), there is no existing
//                  flag-derived topic to reuse, so a stable, deterministic
//                  program-specific topic is generated from the program's own
//                  Code using the same "safety.events." prefix convention
//                  established in EventRoutingTopicCatalog -- not an invented
//                  naming scheme, an extension of the one already in use.
//                  Additive-only evolution -- generated topic strings must
//                  never change once assigned.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Program -> Event routing topic mapping, computed from
/// <see cref="ProgramCatalog.All"/> and, where available, <see cref="EventRoutingTopicCatalog"/>.
/// </summary>
public static class ProgramEventRoutingRules
{
    /// <summary>The event-bus topic the given program's events must be routed to.</summary>
    public static string TopicFor(ProgramDefinition program) =>
        program.PrimaryFlag is SafetyFlag flag
            ? EventRoutingTopicCatalog.TopicForFlag(flag)
            : $"safety.events.program.{program.Code.ToLowerInvariant()}";

    /// <summary>Routing topics for every program in <see cref="ProgramCatalog.All"/>, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, string> ByProgramCode { get; } =
        ProgramCatalog.All.ToDictionary(program => program.Code, TopicFor, StringComparer.Ordinal);
}
