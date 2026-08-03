# Command-Handler Map — Reporting

No domain aggregates, no repository/DbContext writes, no lifecycle of any kind. Inherited from
`docs/ingestion/module-flow/reporting.md`; not independently re-verified this phase.

| Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| N/A — no Create or any other transition exists | N/A | 5 query-style endpoints render catalog/metadata or stream a generated PDF back in the same HTTP response | None — nothing is persisted | None — no event ever published (inherited) | N/A | N/A | Directly verified (inherited) | Reporting's one defining problem per the parent discovery is missing authentication, not lifecycle coverage — there is no lifecycle to audit here, this file exists to close out the 25-file index rather than report a finding |
