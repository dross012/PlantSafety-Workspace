// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure.Options
//  File:           SqlGatewayOptions.cs
//  Created:        2026-08-09
//  Description:    Strongly typed options for SqlServerGateway. Bound from
//                  configuration section "Shared:SqlGateway" -- consistent
//                  with the Section 2.5 rule that all configuration flows
//                  through IConfiguration/strongly-typed options, never
//                  hand-parsed elsewhere. No default connection string is
//                  supplied deliberately: a missing connection string
//                  should fail fast and loudly at startup (see
//                  ServiceCollectionExtensions), not silently point at a
//                  guessed local default.
// ============================================================================
namespace Shared.SqlGateway.Infrastructure.Options;

/// <summary>Strongly typed configuration for the SQL gateway.</summary>
public sealed class SqlGatewayOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Shared:SqlGateway";

    /// <summary>The connection string every ISqlGateway call uses.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Command timeout (seconds) for every stored-procedure call.</summary>
    public int CommandTimeoutSeconds { get; init; } = 60;
}
