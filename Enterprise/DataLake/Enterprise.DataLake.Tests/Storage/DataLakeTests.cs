// ============================================================================
//  Project:        Enterprise.DataLake.Tests
//  File:           DataLakeTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.2 unit tests for DataLakeStore, DataLakeWriter,
//                  and DataLakeQueryEngine: append-only behavior, partition
//                  key derivation, per-type writer methods, and query
//                  filtering.
// ============================================================================
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.DataLake.Infrastructure.Storage;
using Enterprise.DataLake.Infrastructure.Writing;

using FluentAssertions;

using Xunit;

namespace Enterprise.DataLake.Tests.Storage;

public sealed class DataLakeTests
{
    private static readonly DateTime RecordedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void DataLakePartitionKey_For_DerivesTypeAndDate()
    {
        DataLakePartitionKey key = DataLakePartitionKey.For(DataLakeRecordType.Facility, RecordedAt);

        key.RecordType.Should().Be(DataLakeRecordType.Facility);
        key.PartitionDate.Should().Be(new DateOnly(2026, 1, 1));
        key.ToPartitionPath().Should().Be("Facility/2026-01-01");
    }

    [Fact]
    public void DataLakeStore_Append_ThenGetAll_RoundTrips()
    {
        var store = new DataLakeStore();
        var record = new DataLakeRecord(Guid.NewGuid(), DataLakeRecordType.Employee, DataLakePartitionKey.For(DataLakeRecordType.Employee, RecordedAt), Guid.NewGuid(), "payload", RecordedAt);

        store.Append(record);

        store.GetAll().Should().ContainSingle().Which.Should().Be(record);
    }

    [Fact]
    public void DataLakeStore_GetByType_FiltersCorrectly()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        writer.WriteFacilityRecord(Guid.NewGuid(), "facility payload", RecordedAt);
        writer.WriteEmployeeRecord(Guid.NewGuid(), "employee payload", RecordedAt);

        store.GetByType(DataLakeRecordType.Facility).Should().ContainSingle();
        store.GetByType(DataLakeRecordType.Machine).Should().BeEmpty();
    }

    [Fact]
    public void DataLakeWriter_WriteHazardRecord_BlankPayload_Throws()
    {
        var writer = new DataLakeWriter(new DataLakeStore());

        Action act = () => writer.WriteHazardRecord(Guid.NewGuid(), " ", RecordedAt);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DataLakeWriter_EachTypedMethod_WritesCorrectRecordType()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);

        writer.WriteFacilityRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WriteEmployeeRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WriteMachineRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WriteHazardRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WriteEventRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WriteWorkflowRecord(Guid.NewGuid(), "p", RecordedAt);
        writer.WritePredictionRecord(Guid.NewGuid(), "p", RecordedAt);

        IReadOnlyDictionary<DataLakeRecordType, int> counts = new DataLakeQueryEngine(store).CountByType();
        counts.Values.Should().AllSatisfy(count => count.Should().Be(1));
        counts.Keys.Should().BeEquivalentTo(Enum.GetValues<DataLakeRecordType>());
    }

    [Fact]
    public void DataLakeQueryEngine_QueryByPartition_ReturnsOnlyMatchingRecords()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        writer.WriteFacilityRecord(Guid.NewGuid(), "day1", RecordedAt);
        writer.WriteFacilityRecord(Guid.NewGuid(), "day2", RecordedAt.AddDays(1));
        var queryEngine = new DataLakeQueryEngine(store);

        IReadOnlyList<DataLakeRecord> result = queryEngine.QueryByPartition(DataLakePartitionKey.For(DataLakeRecordType.Facility, RecordedAt));

        result.Should().ContainSingle().Which.Payload.Should().Be("day1");
    }

    [Fact]
    public void DataLakeQueryEngine_QueryByDateRange_FiltersInclusively()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        writer.WriteMachineRecord(Guid.NewGuid(), "d1", RecordedAt);
        writer.WriteMachineRecord(Guid.NewGuid(), "d3", RecordedAt.AddDays(2));
        writer.WriteMachineRecord(Guid.NewGuid(), "d5", RecordedAt.AddDays(4));
        var queryEngine = new DataLakeQueryEngine(store);

        IReadOnlyList<DataLakeRecord> result = queryEngine.QueryByDateRange(
            DataLakeRecordType.Machine,
            DateOnly.FromDateTime(RecordedAt),
            DateOnly.FromDateTime(RecordedAt.AddDays(2)));

        result.Should().HaveCount(2);
        result.Select(r => r.Payload).Should().BeEquivalentTo(["d1", "d3"]);
    }

    [Fact]
    public void DataLakeQueryEngine_CountByType_IncludesZeroCountTypes()
    {
        var store = new DataLakeStore();
        var queryEngine = new DataLakeQueryEngine(store);

        IReadOnlyDictionary<DataLakeRecordType, int> counts = queryEngine.CountByType();

        counts.Should().ContainKey(DataLakeRecordType.Prediction).WhoseValue.Should().Be(0);
    }
}
