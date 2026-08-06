// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-05
// FILE DESCRIPTION: Gathers worker-training facts for the Compliance Rules Engine's cross-domain
//   rules (Rules/CrossDomain) by querying the real, SQL-backed Operational.Training read-model
//   (PlantSafety.Application.Operational.Training.Queries.GetTrainingQuery, backed by
//   [Operational].[usp_Training_List] / [Operational].[TrainingReadModel]).
//
//   Deliberately does NOT use the WorkerTrainingRecord aggregate
//   (PlantSafety.Application.Features.Operational.Handlers.Training.ITrainingRepository) --
//   that aggregate's Application/Infrastructure code exists but its backing SQL table and both
//   stored procedures it calls ([Operational].[usp_Training_Assign] /
//   [Operational].[usp_Training_GetById]) do not exist anywhere in this repository. Building on
//   top of it here would mean the rule always evaluates against data that can never actually be
//   persisted. Operational.Training's TrainingItemDto/TrainingReadModel is the one training
//   surface that is genuinely real end-to-end (table + procedure + repository), so it is the
//   only one used here.
//
//   GetTrainingQuery does not support filtering by EmployeeId server-side (no such query
//   exists) -- this provider queries a wide date window and filters client-side by EmployeeId.
//   The underlying [Operational].[TrainingReadModel] table has no dedicated
//   "did this training pass/fail" or per-record expiry column -- Status is a free-text field
//   with no enforced value set anywhere in this codebase. This provider therefore reports only
//   what the real schema can honestly support: whether the worker has *any* training record on
//   file in the window, not a specific course's pass/fail or expiry state. Do not extend this to
//   claim expiry/pass-fail semantics the schema does not carry.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Operational.Training.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real worker-training facts from the Operational.Training read-model.</summary>
public sealed class WorkerTrainingFactsProvider(ISender sender)
{
    /// <summary>
    /// Returns <see langword="true"/> when the given worker has at least one training record on
    /// file (any status) within the lookback window, per the real Operational.Training read-model.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="siteId">Optional site scope.</param>
    /// <param name="workerId">Identity of the worker to check (matched against the read-model's EmployeeId column).</param>
    /// <param name="asOfUtc">The instant to evaluate as "now"; the lookback window ends here.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task<bool> HasTrainingRecordOnFileAsync(
        Guid tenantId,
        Guid? siteId,
        Guid workerId,
        DateTime asOfUtc,
        CancellationToken cancellationToken)
    {
        // A wide, deterministic lookback window -- the real read-model has no unfiltered
        // "all time" query, so a window must be chosen. 10 years covers any realistic
        // certification/training recency requirement without fabricating a narrower cutoff
        // this schema does not itself define.
        var from = asOfUtc.AddYears(-10);

        var result = await sender.Send(
            new GetTrainingQuery(tenantId, siteId, from, asOfUtc),
            cancellationToken);

        if (result.IsFailure)
        {
            // Fail closed: an inability to check training data is not evidence of compliance.
            return false;
        }

        return result.Value.Items.Any(item => item.EmployeeId == workerId);
    }
}
