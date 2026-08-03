# Module Inventory — RiskManagement

Two parallel `Risk`-concept aggregates confirmed (inherited from ingestion discovery, SQL cross-refs
added this phase).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Risk (top-level, `RiskManagement.Aggregates.Risk`) | Create (Identify hazard) | Yes | Yes — `usp_Risk_Insert` (AuditLog-backed) | **Live** — wired to `RiskManagementV1Controller.cs` (prior discovery, controller read in full) | Directly verified (inherited) | |
| Risk | ScoreRisk | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | |
| Risk | Prioritize | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | |
| Risk | ApplyControlMeasure | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | Real cross-field rule: justification required for lower-effectiveness control hierarchy |
| Risk | ValidateEffectiveness | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | |
| Risk | StartReviewCycle / CompleteReview | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | |
| Risk | Close | Yes | Yes — `usp_Risk_Update` | Live | Directly verified (inherited) | |
| Risk | Archive | Yes | Yes — presumed folded into `usp_Risk_Update` | Live | Directly verified (inherited) | Guarded, no `AddDomainEvent` — silent archive, same pattern as Accident |
| Risk | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| RiskManagement (Operational duplicate, `Operational.RiskManagement.Aggregates.RiskManagement`) | Create | Yes (marker aggregate, factory-only) | **Read-only** — the only SQL found for this class is `usp_RiskManagement_List`; **no `_Insert`/`_Create`/`_Update` proc found under this name** | **Legacy/dead-ish** — own controller (`Operational\RiskManagement\Controllers\V1\...`) confirmed GET-only (prior discovery, full read: zero `[HttpPost]`/`[HttpPut]`) | Directly verified (inherited controller read; SQL cross-ref new this phase) | **[NEW]** This phase's SQL check reinforces the "dead duplicate" conclusion: not only is the controller read-only, the SQL layer behind this class has no write path at all — a `Create` call against this aggregate, if one were ever wired, would have nowhere to persist to |
| RiskManagement (Operational duplicate) | Update/Delete/Cancel/status transitions | **No** — none exist | N/A | N/A | Directly verified | |
