// ============================================================================
//  Project:        Enterprise.Integration
//  Module:         Mapping
//  File:           EnterpriseIntegrationMapping.cs
//  Created:        2026-08-08
//  Description:    Phase D.9: a single, explicit field mapping from an
//                  external system's field name to this platform's
//                  internal field name. No reflection, no AutoMapper --
//                  every mapping is a plain, hand-declared record; applying
//                  a set of them is a plain loop (see
//                  EnterpriseIntegrationAdapter.Adapt).
// ============================================================================
namespace Enterprise.Integration.Mapping;

/// <summary>A single, explicit field mapping from an external system's field name to this platform's internal field name.</summary>
public sealed record EnterpriseIntegrationMapping(
    string ExternalFieldName,
    string InternalFieldName);
