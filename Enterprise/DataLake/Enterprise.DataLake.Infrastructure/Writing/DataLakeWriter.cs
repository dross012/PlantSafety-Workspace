// ============================================================================
//  Project:        Enterprise.DataLake.Infrastructure
//  Module:         Writing
//  File:           DataLakeWriter.cs
//  Created:        2026-08-08
//  Description:    Phase D.2: the outward-facing write API over
//                  IDataLakeStore -- one typed method per record kind Path
//                  D's spec requires the lake to store (Facility/Employee/
//                  Machine/Hazard/Event/Workflow/Prediction), each
//                  deterministically deriving the record's partition key
//                  from its type and recorded-at timestamp. Callers outside
//                  Enterprise.DataLake depend on this, not on IDataLakeStore
//                  directly.
// ============================================================================
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Storage;

namespace Enterprise.DataLake.Infrastructure.Writing;

/// <summary>The outward-facing write API over the enterprise data lake.</summary>
public interface IDataLakeWriter
{
    /// <summary>Writes a Facility record (Path B) and returns it.</summary>
    DataLakeRecord WriteFacilityRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes an Employee record (Path C) and returns it.</summary>
    DataLakeRecord WriteEmployeeRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes a Machine record (Path C) and returns it.</summary>
    DataLakeRecord WriteMachineRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes a Hazard/hazard-propagation record (Path B/C) and returns it.</summary>
    DataLakeRecord WriteHazardRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes a correlated domain-event record (Path B/C) and returns it.</summary>
    DataLakeRecord WriteEventRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes a Workflow record (Path C) and returns it.</summary>
    DataLakeRecord WriteWorkflowRecord(Guid sourceId, string payload, DateTime recordedAtUtc);

    /// <summary>Writes a predictive-model-output record (Path C) and returns it.</summary>
    DataLakeRecord WritePredictionRecord(Guid sourceId, string payload, DateTime recordedAtUtc);
}

/// <summary><see cref="IDataLakeWriter"/> implementation over <see cref="IDataLakeStore"/>.</summary>
public sealed class DataLakeWriter(IDataLakeStore store) : IDataLakeWriter
{
    /// <inheritdoc />
    public DataLakeRecord WriteFacilityRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Facility, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WriteEmployeeRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Employee, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WriteMachineRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Machine, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WriteHazardRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Hazard, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WriteEventRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Event, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WriteWorkflowRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Workflow, sourceId, payload, recordedAtUtc);

    /// <inheritdoc />
    public DataLakeRecord WritePredictionRecord(Guid sourceId, string payload, DateTime recordedAtUtc) =>
        Write(DataLakeRecordType.Prediction, sourceId, payload, recordedAtUtc);

    private DataLakeRecord Write(DataLakeRecordType recordType, Guid sourceId, string payload, DateTime recordedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("payload must not be null or whitespace.", nameof(payload));
        }

        var record = new DataLakeRecord(
            RecordId: Guid.NewGuid(),
            RecordType: recordType,
            PartitionKey: DataLakePartitionKey.For(recordType, recordedAtUtc),
            SourceId: sourceId,
            Payload: payload,
            RecordedAtUtc: recordedAtUtc);

        store.Append(record);
        return record;
    }
}
