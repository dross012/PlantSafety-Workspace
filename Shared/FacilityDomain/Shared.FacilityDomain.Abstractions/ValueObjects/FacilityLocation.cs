// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  File:           FacilityLocation.cs
//  Created:        2026-08-08
//  Description:    A facility's physical address. Structured (rather than
//                  the free-text "Location" string every prior read-model
//                  used -- e.g. TenantFacilityRegistration.Location) since
//                  this is the real aggregate now, not a derived view.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>A facility's physical address.</summary>
public sealed record FacilityLocation(
    string AddressLine1,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryCode);
