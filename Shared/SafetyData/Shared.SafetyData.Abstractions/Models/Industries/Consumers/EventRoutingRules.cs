// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           EventRoutingRules.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Event routing topic mapping,
//                  derived entirely from IndustryDefinition.Flags (SafetyFlag)
//                  via IndustryRegistry.All. Introduces no new industries,
//                  regulators, or flags -- EventRoutingTopicCatalog.TopicForFlag
//                  is a total, 1:1 mapping from each existing SafetyFlag bit to
//                  a stable event-bus topic name. Mirrors HazardMapping.cs's
//                  shape, but also exposes the inverse view (topic -> the
//                  industries that route to it) since routing rules are
//                  naturally consumed topic-first by subscribers. Additive-only
//                  evolution -- topic name strings must never change once
//                  assigned (renaming a topic is a breaking change for any
//                  subscriber), and TopicForFlag must remain total over every
//                  SafetyFlagCatalog.AllFlags value.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>Total, 1:1 mapping from each single-bit <see cref="SafetyFlag"/> to its event-bus topic name.</summary>
public static class EventRoutingTopicCatalog
{
    /// <summary>Returns the stable event-bus topic name corresponding to a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static string TopicForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => "safety.events.machinery",
        SafetyFlag.Chemicals => "safety.events.chemicals",
        SafetyFlag.Flammables => "safety.events.flammables",
        SafetyFlag.HazmatShipping => "safety.events.hazmat-shipping",
        SafetyFlag.Fleet => "safety.events.fleet",
        SafetyFlag.EnvironmentalPermits => "safety.events.environmental-permits",
        SafetyFlag.ExplosionRisk => "safety.events.explosion-risk",
        SafetyFlag.ConfinedSpaces => "safety.events.confined-spaces",
        SafetyFlag.LOTO => "safety.events.loto",
        SafetyFlag.Dust => "safety.events.dust",
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };
}

/// <summary>
/// An event routing rule: the industries whose events must be published to a given topic.
/// </summary>
/// <param name="Topic">The stable event-bus topic name.</param>
/// <param name="IndustryNames">Names of every industry (<see cref="IndustryDefinition.Name"/>) whose <see cref="SafetyFlag"/> bitmask includes the flag this topic was derived from, in <see cref="IndustryRegistry.All"/> order.</param>
public sealed record EventRoutingRule(string Topic, IReadOnlyList<string> IndustryNames);

/// <summary>
/// Deterministic Industry -> Event routing topic mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Flags"/>.
/// </summary>
public static class EventRoutingRules
{
    /// <summary>One <see cref="EventRoutingRule"/> per <see cref="SafetyFlagCatalog.AllFlags"/> value, in that order.</summary>
    public static IReadOnlyList<EventRoutingRule> All { get; } = SafetyFlagCatalog.AllFlags
        .Select(flag => new EventRoutingRule(
            EventRoutingTopicCatalog.TopicForFlag(flag),
            IndustryRegistry.All.Where(industry => industry.Flags.HasFlag(flag)).Select(industry => industry.Name).ToArray()))
        .ToArray();

    /// <summary>Every topic the given industry's events must be routed to, in <see cref="SafetyFlagCatalog.AllFlags"/> order.</summary>
    public static IReadOnlyList<string> TopicsFor(IndustryDefinition industry) =>
        SafetyFlagCatalog.Decompose(industry.Flags).Select(EventRoutingTopicCatalog.TopicForFlag).ToArray();

    /// <summary>Routing topics for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            TopicsFor,
            StringComparer.Ordinal);
}
