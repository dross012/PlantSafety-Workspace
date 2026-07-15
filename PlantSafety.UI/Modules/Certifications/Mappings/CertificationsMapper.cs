// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Maps each of the 7 certification category item DTOs into
//   the single display-normalized CertificationRowViewModel so
//   CertificationsListPage can render every category through one
//   DataGridComponent column set (Platform UI Architecture Master
//   Specification §4 — components stay pure/presentational; mapping lives in
//   the module's own Mappings layer, not in the grid or the API client).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Certifications.Models;

namespace PlantSafety.UI.Modules.Certifications.Mappings;

/// <summary>Maps category-specific certification item DTOs to the shared display row view model.</summary>
public static class CertificationsMapper
{
    /// <summary>Maps contractor certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<ContractorCertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "contractor",
            CategoryLabel = "Contractor",
            HolderLabel = i.ContractorName,
            SubTypeLabel = i.CertificationType,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps DOT certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<DotCertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "dot",
            CategoryLabel = "DOT",
            HolderLabel = i.DriverName,
            SubTypeLabel = i.LicenseClass,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps EPA certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<EpaCertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "epa",
            CategoryLabel = "EPA",
            HolderLabel = i.PermitHolderName,
            SubTypeLabel = i.EpaProgram,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps industry-specific certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<IndustrySpecificCertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "industryspecific",
            CategoryLabel = "Industry-Specific",
            HolderLabel = i.HolderName,
            SubTypeLabel = i.IndustryStandard,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps ISO 14001 certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<Iso14001CertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "iso14001",
            CategoryLabel = "ISO 14001",
            HolderLabel = i.CertifyingBody,
            SubTypeLabel = i.Scope,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps ISO 45001 certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<Iso45001CertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "iso45001",
            CategoryLabel = "ISO 45001",
            HolderLabel = i.CertifyingBody,
            SubTypeLabel = i.Scope,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Maps OSHA certification records.</summary>
    public static IReadOnlyList<CertificationRowViewModel> ToRows(IReadOnlyList<OshaCertItemDto> items) =>
        items.Select(i => new CertificationRowViewModel
        {
            CertificationId = i.CertificationId,
            CategoryKey = "osha",
            CategoryLabel = "OSHA",
            HolderLabel = i.HolderName,
            SubTypeLabel = i.OshaStandard,
            Status = i.Status,
            IssuedAtUtc = i.IssuedAtUtc,
            ExpiresAtUtc = i.ExpiresAtUtc,
            VerifiedBy = i.VerifiedBy,
            SiteId = i.SiteId,
        }).ToList();

    /// <summary>Gets the ordered list of (CategoryKey, DisplayLabel) pairs backing the certification tabs.</summary>
    public static IReadOnlyList<(string Key, string Label)> Categories { get; } =
    [
        ("osha", "OSHA"),
        ("contractor", "Contractor"),
        ("dot", "DOT"),
        ("epa", "EPA"),
        ("industryspecific", "Industry-Specific"),
        ("iso14001", "ISO 14001"),
        ("iso45001", "ISO 45001"),
    ];
}
