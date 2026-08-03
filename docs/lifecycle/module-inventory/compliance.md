# Module Inventory — Compliance

7 create-only aggregates under `PlantSafety.Domain.Compliance`: `Auditing` (marker, duplicate of the
live `Auditing.AuditEngagement`/`AuditFinding` — see `auditing.md`), `Certification`, `Compliance`,
`ManagementOfChange`, `Permits`, `Policy`, `Procedures`, `RegulatoryCalendar`. **[NEW this phase]**: a
precise, exact-prefix SQL cross-reference (not attempted by the prior discovery) found **no genuine
`usp_<ClassName>_Create/Insert/Update` proc for any of the 6 non-`Auditing` aggregates in this cluster**
— only adjacent read-side reporting procs with related-but-different compound names.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Certification | Create only | Yes | **No genuine write proc found** — only subtype read reports exist: `usp_CertificationISO45001_GetISO45001`, `_ISO14001`, `_DOT`, `_EPA`, `_Contractor`, `_OSHACertifications`, `_Training`, `_IndustrySpecific` (all `_Get*`, read-only, and named for a *subtype*, not the base aggregate) | Unconfirmed — plausibly dead/unwired | Directly verified (file-name census); **not** confirmed by reading the actual repository/handler code, so a different write-proc name is possible | |
| Compliance | Create only | Yes | **No genuine write proc found** — `usp_ComplianceAudit_GetAudits`, `usp_ComplianceExportItems_List`, `usp_ComplianceFailure_List`, `usp_ComplianceResources_BulkUpdate` exist but are reporting/export procs, not a `Compliance`-aggregate Create/Insert | Unconfirmed | Directly verified (file-name census) | `usp_ComplianceResources_BulkUpdate` is the closest candidate for a real write path but its target table was not confirmed this phase |
| ManagementOfChange | Create only | Yes | **No genuine write proc found** — `usp_MOCApprovals_GetApprovals`, `usp_MOCRiskAssessments_GetRiskAssessments`, `usp_MOCWorkflows_GetMOCWorkflows` are all read-only reports on MOC sub-concepts | Unconfirmed | Directly verified (file-name census) | |
| Permits (Compliance) | Create only | Yes | **No genuine write proc found** — only `usp_PermitsToWork_List` (read-only; also note this is a *different* SQL-visible concept name than the C# class `Permits`) | Unconfirmed | Directly verified (file-name census) | |
| Policy | Create only | Yes | **No genuine write proc found for the base `Policy` aggregate** — 12 `usp_Policy*` procs exist (`PolicyAcknowledgment_Insert`, `PolicyApprovals_GetApprovals`, `PolicyChemicalHandling_List`, `PolicyContractor_List`, `PolicyDocuments_List`, `PolicyEmergency_List`, `PolicyEnvironmental_List`, `PolicyEquipment_List`, `PolicyHR_List`, `PolicyPPE_List`, `PolicyRevisions_List`, `PolicySafetyPolicies_List`) but every one is either a child/acknowledgment table or a category-scoped read report — none is `usp_Policy_Create`/`_Insert`/`_Update` | Unconfirmed | Directly verified (file-name census) | The largest SQL footprint of any create-only aggregate in the solution, and still no confirmed write path for the base entity itself |
| Procedures | Create only | Yes | **Read-only** — `usp_Procedures_GetItems` only | Unconfirmed | Directly verified | |
| RegulatoryCalendar | Create only | Yes | **No SQL of any kind found** under this name or a loose substring match | Unconfirmed — candidate for the zero-SQL orphan list in `inventory.md` | Directly verified | |
| Auditing (marker, `Compliance.Auditing.Aggregates.Auditing`) | Create only | Yes | **Read-only** — `usp_Auditing_GetLogs` only | **Legacy/dead-ish** — one of the 22 no-op-consumer pairs (inherited); own Create path confirmed to have no write proc this phase | Directly verified (SQL check new this phase) | Duplicate of the live `AuditEngagement`/`AuditFinding` (see `auditing.md`) |
| All 7 aggregates above | Update/Delete/Cancel/status transitions | **No** — none exist for any of the 7 | N/A | N/A | Directly verified | This is the largest concentrated cluster of "Create-only aggregate with an apparently write-orphaned Create" in the solution — see `inventory.md` §1 for how this reframes the C#-only-orphan count |

**Caveat, repeated from `inventory.md`:** "No genuine write proc found" here means file-name matching
found none — it does not rule out a differently-named proc this pass's grep pattern didn't anticipate,
and no stored procedure body or repository implementation was opened to confirm. Recommended as a
Phase 2 priority: open `Compliance.Application`'s repository implementations directly for these 6
aggregates and confirm what stored procedure (if any) their `Create` command handler actually calls.
