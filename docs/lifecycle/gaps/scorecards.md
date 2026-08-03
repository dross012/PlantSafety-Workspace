# Gaps — Scorecards

1 aggregate at top level (`Scorecards.Scorecard`); see `analytics.md` for its create-only duplicate
marker (`Analytics.ScoreCards`). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| Scorecard | Cancel/Delete missing `[Inv]` | No — real business rule: `CompleteCompilation` rejects a scorecard with zero recorded measurements; `RecordDistribution` has a cosmetic exception-message copy-paste bug (compares `Status.ToString()` to itself in the message text, not the guard condition) — not a guard bypass `[Inv]` | **Yes** — `Archive` confirmed to never call `AddDomainEvent`, the 4th and final confirmed silent-archive instance `[DM-F3]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (named in the 23-aggregate cluster) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |

**Net for this module**: closes out the silent-archive population at exactly 4 confirmed instances (Accident, Risk, Scorecard, SafetyPolicy-presumed) out of 8 total Archive-havers in the solution — the other 4 (PermitTemplate, Dashboard, Operational.Alerts.Alert confirmed correct; PermitWorkflow suspected but unconfirmed either way).
