# Gap-File Citation Legend (Phase 8)

Every `gaps/*.md` file cites findings using these short codes instead of re-arguing them. Full text
lives at the cited path. This is a synthesis phase — no new investigation was performed; where a
per-aggregate fact was not directly established by Phases 1-7, the row says "Pattern-inferred" or
"Not individually confirmed" exactly as the source phase scored it.

| Code | Source | Subject |
|---|---|---|
| `[Inv]` | `inventory.md` + `module-inventory/<name>.md` | Phase 1: transition census (Create/Update/Archive/Cancel/Delete presence) |
| `[DM-F1]`…`[DM-F6]` | `domain-mapping/_shared-findings.md` | Phase 2: tenancy gap (F1), Intelligence handler-bypass (F2), silent-archive (F3), Alert duplicate (F4), PermitToWork/PermitWorkflow overlap (F5), AuditEngagement.Close cross-module gap (F6) |
| `[CH-F7]`…`[CH-F13]` | `command-handler-map/_shared-findings.md` | Phase 3: Outbox universality (F7), AuditLog 23/93 coverage + best-effort/anonymous (F8), zero-consumer finding (F9), handler shape (F10), EscalateToIncident (F11), ChecklistCompletion/Template (F12), RegulatoryCalendar zero-impl (F13) |
| `[RE-G1]`…`[RE-G6]` | `rule-enforcement/_shared-findings.md` | Phase 4: F1 extends to DataLake/Intelligence (G1), the two distinct actor bugs (G2), validator coverage 190/220 (G3), lifecycle guards are real (G4), F6/F11/F12/F13 reconfirmed (G5), F2 reconfirmed (G6) |
| `[EV-F14]`…`[EV-F17]` | `event-map/_shared-findings.md` | Phase 5: uniform event shape (F14), no old-value field (F15), Outbox envelope actor/correlation always wrong (F16), no new duplicate-publish (F17) |
| `[SQL-G1]`…`[SQL-G5]` | `sql-shape/_shared-findings.md` | Phase 6: 13-aggregate zero-implementation (G1), naming-collision trap (G2), truncation count (G3), enum drift (G4), G1 ranked worst overall (G5) |
| `[RLS§1-6]` | `security/rls-audit.md` | Phase 7: SESSION_CONTEXT mechanism (§1), confirmed-blind classic-pattern aggregates (§2), Features/* pattern that does engage (§3), not client-spoofable (§4), DataLake/Intelligence/Azure (§5), auth-500-fix orthogonality (§6) |
| `[Actor§1-5]` | `security/actor-propagation.md` | Phase 7: the two bugs restated (§1), 23-aggregate both-bugs cluster (§2a), ~35-aggregate Bug#1-only cluster (§2b), zero-impl aggregates never reach either bug (§2c), DataLake/Intelligence/Azure third failure shape (§3) |

**Standard per-row values used throughout, to avoid re-deriving the same sentence 90 times:**

- **Missing Transitions**: cites `[Inv]` + the specific missing verb(s) (Delete/Cancel/Archive/Update all missing for Create-only aggregates).
- **Bypasses Domain Rules**: `Yes` only for Intelligence's 2 aggregates `[DM-F2]`; `No` (guard genuinely executes) for every other aggregate checked `[RE-G4]`/`[DM-F2]`; `Partial` for the 4 same-aggregate/cross-module gaps named in F6/F11/F12/F13/RE-G5.
- **Fails to Publish Events**: `Yes` for the 4 silent-Archive aggregates `[DM-F3]`; `N/A (inherent)` for Create-only aggregates (nothing to grow beyond Create) and for zero-SQL-implementation aggregates; `No` (correctly instrumented) for the 3 confirmed non-silent Archive-havers; `Yes (12 of 13 transitions)` for Intelligence, whose guard never runs so no event is ever constructed `[DM-F2]`.
- **Violates Tenancy**: `Yes` for the classic `GetByIdAsync(id)`/`UpdateAsync(aggregate)` shape (the ~46+23 rich-aggregate population) `[DM-F1]`/`[RE-G1]`; `N/A` for Create-only aggregates with no load-by-ID mutation to violate (their `AddAsync`/Create path does carry TenantId); `Partial` for the newer `Features/*` repository generation, which does check app-layer but the check is itself unenforced per F1's own framing.
- **Violates Actor Propagation**: `Yes — Bug#1+#2` for the 23-aggregate cluster `[Actor§2a]`; `Yes — Bug#1 only` for the ~35 other real, implemented write-repositories `[Actor§2b]`; `N/A — never reaches a repository` for the 13 zero-implementation aggregates `[Actor§2c]`/`[SQL-G1]`; module-specific third shape for DataLake/Intelligence/Azure `[Actor§3]`.
- **Violates SQL Shape**: `Yes — zero implementation` for the 13 `[SQL-G1]`-named aggregates; `Yes — truncation` only for `Alert`/`TriggerAlertCommand` `[SQL-G3]`; otherwise `No confirmed shape violation` (structurally-possible-but-unconfirmed enum drift `[SQL-G4]` noted where relevant, not scored as a violation).
- **Violates RLS**: `Yes — SESSION_CONTEXT never engages` for every classic-pattern `GetByIdAsync(id)`/`UpdateAsync(aggregate)` transition `[RLS§2]`; `No (engages correctly, but on 13 aggregates this is coincidental — see SQL-G1)` for the `Features/*`-pattern aggregates `[RLS§3]`; `N/A` for Create-only aggregates' sole transition (Create/AddAsync carries TenantId, RLS engages there) `[RLS§2]` closing note.
