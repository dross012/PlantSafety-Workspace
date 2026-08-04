// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           IDisposalCertificateDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Marker interface for all DisposalCertificate domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Events;

/// <summary>Marker interface for all DisposalCertificate domain events.</summary>
public interface IDisposalCertificateDomainEvent : IDomainEvent { }
