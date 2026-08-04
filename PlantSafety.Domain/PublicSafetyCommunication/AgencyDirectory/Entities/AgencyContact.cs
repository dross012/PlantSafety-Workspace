// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           AgencyContact.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the PublicSafetyAgency aggregate representing a single named
//                  contact/desk within an external public-safety agency (dispatch center,
//                  commander, liaison officer, regulatory contact, after-hours line).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Entities;

/// <summary>A single named contact/desk within a <see cref="PublicSafetyAgency"/>.</summary>
public sealed class AgencyContact
{
    /// <summary>Unique identity of this contact record.</summary>
    public AgencyContactId AgencyContactId { get; private set; }

    /// <summary>Identity of the parent PublicSafetyAgency aggregate.</summary>
    public PublicSafetyAgencyId PublicSafetyAgencyId { get; private set; }

    /// <summary>Role this contact plays within the agency's response structure.</summary>
    public AgencyContactRole Role { get; private set; }

    /// <summary>Display name of the contact or desk (e.g., "County Dispatch Center").</summary>
    public string ContactName { get; private set; } = string.Empty;

    /// <summary>Primary phone number for this contact.</summary>
    public string PhoneNumber { get; private set; } = string.Empty;

    /// <summary>Email address for this contact, if any.</summary>
    public string? Email { get; private set; }

    /// <summary>UTC timestamp this contact record was last verified as current.</summary>
    public UtcTimestamp? LastVerifiedAtUtc { get; private set; }

    /// <summary>UTC timestamp this contact record was added.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>For ORM/aggregate rehydration only.</summary>
    private AgencyContact() { }

    /// <summary>Creates a new AgencyContact record.</summary>
    public AgencyContact(
        AgencyContactId agencyContactId,
        PublicSafetyAgencyId publicSafetyAgencyId,
        AgencyContactRole role,
        string contactName,
        string phoneNumber,
        string? email,
        UtcTimestamp createdAtUtc)
    {
        AgencyContactId = agencyContactId;
        PublicSafetyAgencyId = publicSafetyAgencyId;
        Role = role;
        ContactName = (contactName ?? string.Empty).Trim();
        PhoneNumber = (phoneNumber ?? string.Empty).Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Records that this contact was verified as current/reachable.</summary>
    public void RecordVerified(UtcTimestamp verifiedAtUtc) => LastVerifiedAtUtc = verifiedAtUtc;
}
