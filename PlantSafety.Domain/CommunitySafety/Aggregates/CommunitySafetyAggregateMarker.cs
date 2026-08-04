// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety
//  Layer:          Domain
//  File:           CommunitySafetyAggregateMarker.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Namespace marker for top-level CommunitySafety aggregates. CommunitySafety
//                  manages the plant's safety relationship with surrounding communities:
//                  community contact registry, complaint/concern intake, community impact
//                  tracking for on-site incidents, and outreach/notification history. Each
//                  sub-component (CommunityContact, CommunityConcern, CommunityImpactAssessment,
//                  CommunityNotification, CommunityMeetingRecord) owns its own aggregate root
//                  under this namespace root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.Aggregates;

/// <summary>Namespace marker for top-level CommunitySafety aggregates.</summary>
internal static class CommunitySafetyAggregateMarker { }
