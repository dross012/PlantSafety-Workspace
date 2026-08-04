// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           MutualAidAgreement.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity representing a mutual-aid agreement with an external local
//                  emergency responder (fire/police/EMS/mutual-aid). Agreement documents are
//                  version-controlled with effective-date history per the module's data
//                  governance requirement -- DocumentVersion/EffectiveFromUtc capture that
//                  history at the entity level.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

/// <summary>A mutual-aid agreement with an external local emergency response agency.</summary>
public sealed class MutualAidAgreement
{
    /// <summary>Unique identity of this agreement.</summary>
    public MutualAidAgreementId MutualAidAgreementId { get; private set; }

    /// <summary>Identity of the parent LocalEmergencyCoordination aggregate.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; private set; }

    /// <summary>Name of the external agency (fire, police, EMS, mutual-aid partner).</summary>
    public string AgencyName { get; private set; } = string.Empty;

    /// <summary>Discipline of the responding agency (e.g., Fire, Police, EMS, MutualAid).</summary>
    public string Discipline { get; private set; } = string.Empty;

    /// <summary>Document version identifier for the agreement text (version-controlled per data governance).</summary>
    public int DocumentVersion { get; private set; }

    /// <summary>UTC instant this agreement version became effective.</summary>
    public UtcTimestamp EffectiveFromUtc { get; private set; }

    /// <summary>UTC instant this agreement is next due for review/renewal.</summary>
    public UtcTimestamp RenewalDueAtUtc { get; private set; }

    /// <summary>Current renewal/currency status.</summary>
    public MutualAidAgreementRenewalStatus RenewalStatus { get; private set; }

    /// <summary>UTC instant the agreement was established.</summary>
    public UtcTimestamp EstablishedAtUtc { get; private set; }

    /// <summary>Identity of the user who established the agreement.</summary>
    public UserId EstablishedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private MutualAidAgreement() { }

    /// <summary>Creates a new MutualAidAgreement child entity.</summary>
    internal static MutualAidAgreement Create(
        LocalEmergencyCoordinationId localEmergencyCoordinationId,
        string agencyName,
        string discipline,
        int documentVersion,
        UtcTimestamp effectiveFromUtc,
        UtcTimestamp renewalDueAtUtc,
        UtcTimestamp establishedAtUtc,
        UserId establishedBy)
        => new()
        {
            MutualAidAgreementId         = MutualAidAgreementId.New(),
            LocalEmergencyCoordinationId = localEmergencyCoordinationId,
            AgencyName                   = (agencyName ?? string.Empty).Trim(),
            Discipline                   = (discipline ?? string.Empty).Trim(),
            DocumentVersion              = documentVersion < 1 ? 1 : documentVersion,
            EffectiveFromUtc             = effectiveFromUtc,
            RenewalDueAtUtc              = renewalDueAtUtc,
            RenewalStatus                = MutualAidAgreementRenewalStatus.Current,
            EstablishedAtUtc             = establishedAtUtc,
            EstablishedBy                = establishedBy,
        };

    /// <summary>Recomputes <see cref="RenewalStatus"/> against the supplied current instant per <see cref="PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.MutualAidAgreementRenewalPolicy"/>.</summary>
    internal void RefreshRenewalStatus(UtcTimestamp asOfUtc)
        => RenewalStatus = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.MutualAidAgreementRenewalPolicy.Evaluate(RenewalDueAtUtc, asOfUtc);
}
