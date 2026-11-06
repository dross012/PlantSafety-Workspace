// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Contracts
//  File:           IContract.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. The opt-in marker every NEW
//                  contract going forward may implement to carry
//                  ContractMetadata. Deliberately not retrofitted onto
//                  CallerContext, TenantContext, WorkflowContext,
//                  WorkflowDecision, AuditLogSearchCriteria, ErrorEnvelope,
//                  or any other already-merged, already-tested record from
//                  Phases 2-11: changing their base type/shape would be a
//                  breaking constructor-signature change to code already
//                  relied on across dozens of call sites, for no functional
//                  benefit -- exactly the "must not introduce new business
//                  logic" constraint this phase's own instructions impose.
//                  This interface exists so future contracts converge on
//                  one shape from the start, not so past ones are rewritten.
// ============================================================================
namespace Shared.Authorization.Abstractions.Contracts;

/// <summary>Marks a type as carrying the unified ContractMetadata bundle.</summary>
public interface IContract
{
    /// <summary>The unified metadata (tenant, caller, correlation, timestamp) this contract carries.</summary>
    ContractMetadata Metadata { get; }
}
