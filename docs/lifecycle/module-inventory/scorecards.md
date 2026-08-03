# Module Inventory — Scorecards

1 aggregate at top level (`Scorecards.Scorecard`); see `analytics.md` for a create-only duplicate marker
(`Analytics.ScoreCards`).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Scorecard | Create (Draft) | Yes | Yes — `usp_Scorecard_Insert` (AuditLog-backed) | Live | Directly verified (inherited) | |
| Scorecard | RecordMetric | Yes | Yes — `usp_Scorecard_Update` | Live | Directly verified (inherited) | |
| Scorecard | CompleteCompilation | Yes | Yes — `usp_Scorecard_Update` | Live | Directly verified (inherited) | Real business rule: rejects compiling a scorecard with zero recorded measurements |
| Scorecard | Publish | Yes | Yes — `usp_Scorecard_Update` | Live | Directly verified (inherited) | |
| Scorecard | RecordDistribution | Yes | Yes — `usp_Scorecard_Update` | Live | Directly verified (inherited) | Exception-message copy-paste bug (`Status.ToString()` compared to itself in the message, not the guard) — cosmetic, not a guard bypass |
| Scorecard | Close | Yes | Yes — `usp_Scorecard_Update` | Live | Directly verified (inherited) | |
| Scorecard | Archive | Yes | Yes — presumed folded into `usp_Scorecard_Update` | Live | Directly verified (inherited) | Confirmed no `AddDomainEvent` — 4th confirmed instance of the silent-archive pattern |
| Scorecard | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
