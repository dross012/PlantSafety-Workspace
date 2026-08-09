// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Actions
//  File:           EnterpriseRuleAction.cs
//  Created:        2026-08-08
//  Description:    Phase D.5: a single action an EnterpriseRule triggers
//                  once every one of its conditions is satisfied.
// ============================================================================
namespace Enterprise.RulesEngine.Abstractions.Actions;

/// <summary>A single action triggered when an <c>EnterpriseRule</c>'s conditions are all satisfied.</summary>
public sealed record EnterpriseRuleAction(
    string ActionCode,
    string Description);
