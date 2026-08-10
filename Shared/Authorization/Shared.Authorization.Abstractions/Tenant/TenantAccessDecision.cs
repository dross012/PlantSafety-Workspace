// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantAccessDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. TenantIsolationCheck.Evaluate's
//                  return type.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The outcome of a single tenant-isolation check.</summary>
public sealed record TenantAccessDecision(bool IsAuthorized, string Reason);
