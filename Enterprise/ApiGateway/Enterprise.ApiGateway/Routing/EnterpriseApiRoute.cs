// ============================================================================
//  Project:        Enterprise.ApiGateway
//  Module:         Routing
//  File:           EnterpriseApiRoute.cs
//  Created:        2026-08-08
//  Description:    Phase D.3 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): centralized, deterministic route
//                  constants for every unified endpoint Path D's spec
//                  requires. EnterpriseApiGatewayController's [Route]/
//                  [HttpGet] attributes reference these rather than
//                  repeating literal path segments, so the route table
//                  has exactly one source of truth.
// ============================================================================
namespace Enterprise.ApiGateway.Routing;

/// <summary>Centralized route constants for the enterprise API gateway.</summary>
public static class EnterpriseApiRoute
{
    /// <summary>The gateway's base route.</summary>
    public const string Base = "api/v1/enterprise";

    /// <summary>Relative route segment for facilities.</summary>
    public const string Facilities = "facilities";

    /// <summary>Relative route segment for employees.</summary>
    public const string Employees = "employees";

    /// <summary>Relative route segment for machines.</summary>
    public const string Machines = "machines";

    /// <summary>Relative route segment for hazards.</summary>
    public const string Hazards = "hazards";

    /// <summary>Relative route segment for events.</summary>
    public const string Events = "events";

    /// <summary>Relative route segment for workflows.</summary>
    public const string Workflows = "workflows";

    /// <summary>Relative route segment for predictions.</summary>
    public const string Predictions = "predictions";

    /// <summary>Relative route segment for the unified enterprise graph.</summary>
    public const string Graph = "graph";
}
