# Gaps — Reporting

No domain aggregates, no repository/DbContext writes, no lifecycle of any kind. See `_legend.md`.

| Concept | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| N/A (no aggregates) | N/A — there is no Create to grow from; every endpoint returns catalog/metadata or streams a rendered PDF in the same HTTP response, nothing persisted `[Inv]` | N/A — no domain rules exist to bypass | N/A — no events, no persistence | N/A — no tenant-scoped state exists to violate | N/A — no transitions exist for actor/correlation to attach to | N/A — no SQL destination exists | N/A — no RLS-relevant state exists |

**Net for this module**: Reporting is out of scope for every one of this phase's 7 gap categories by construction — it has no lifecycle to have gaps in. Per the parent Ingestion Discovery, this module's one defining problem is **missing authentication**, not lifecycle coverage — worth flagging to Phase 9 as a different kind of finding (an availability/security gap orthogonal to this audit's transition-coverage focus, not a "0 of 7 categories addressed" module).
