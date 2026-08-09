// ============================================================================
//  Project:        Enterprise.Integration
//  Module:         Events
//  File:           EnterpriseIntegrationEvent.cs
//  Created:        2026-08-08
//  Description:    Phase D.9 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): a single inbound event from an external
//                  enterprise system, after field mapping. Unlike
//                  Enterprise.DataLake's deliberately opaque string payload
//                  (Phase D.2), this project's whole purpose is field
//                  mapping, so Payload stays structured (mapped internal
//                  field name -> value) to make that mapping meaningful.
// ============================================================================
namespace Enterprise.Integration.Events;

/// <summary>The external enterprise system a <see cref="EnterpriseIntegrationEvent"/> originated from.</summary>
public enum EnterpriseIntegrationSystem
{
    /// <summary>A Human Resources system.</summary>
    HR,

    /// <summary>An Enterprise Resource Planning system.</summary>
    ERP,

    /// <summary>A Manufacturing Execution System.</summary>
    MES,

    /// <summary>A Computerized Maintenance Management System.</summary>
    CMMS,

    /// <summary>An Environment, Health, and Safety system.</summary>
    EHS,
}

/// <summary>A single inbound event from an external enterprise system, after deterministic field mapping.</summary>
public sealed record EnterpriseIntegrationEvent(
    Guid EventId,
    EnterpriseIntegrationSystem SourceSystem,
    string EventType,
    IReadOnlyDictionary<string, string> MappedPayload,
    DateTime OccurredAtUtc);
