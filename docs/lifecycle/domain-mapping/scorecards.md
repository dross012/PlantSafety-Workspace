# Domain Mapping — Scorecards

See `_shared-findings.md` §F1 (tenancy), §F3 (silent archive).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Scorecard | Create (Draft) | Starts `Draft` | Y (trivial) | Required fields | None | Not enforced (F1) | Directly verified (inherited) | |
| Scorecard | RecordMetric | Must be `Draft`/compiling (pattern-inferred) | Pattern-inferred Y | Metric value required | None | Not enforced (F1) | Directly verified (inherited) | |
| Scorecard | CompleteCompilation | Rejects compiling a scorecard with zero recorded measurements (Phase 1 finding) | **Y** — confirmed real business rule | — | None | Not enforced (F1) | Directly verified (inherited) | Positive control |
| Scorecard | Publish | Must be compiled | Y (pattern-inferred) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Scorecard | RecordDistribution | Guard exists; exception-message copy-paste bug (`Status.ToString()` compared to itself in the message text, not the guard condition — Phase 1 finding) | Y (guard is real; the bug is cosmetic/diagnostic only, not a bypass) | — | None | Not enforced (F1) | Directly verified (inherited) | Cosmetic bug, does not weaken enforcement |
| Scorecard | Close | Must be published/distributed (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Scorecard | Archive | Must be `Closed`; **confirmed no `AddDomainEvent`** — 4th confirmed silent-archive instance (Phase 1 finding, inherited) | Y (guard), **N** (event) | None | None | Not enforced (F1) | Directly verified (inherited) | |
| Scorecard | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** Same silent-archive category as Accident/Risk; CompleteCompilation's zero-measurement
guard is a genuine positive control worth preserving as a model for other "finalize" transitions.
