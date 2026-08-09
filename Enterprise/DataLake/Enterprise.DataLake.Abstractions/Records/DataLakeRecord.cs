// ============================================================================
//  Project:        Enterprise.DataLake
//  Module:         Abstractions.Records
//  File:           DataLakeRecord.cs
//  Created:        2026-08-08
//  Description:    Phase D.2: a single immutable record in the enterprise
//                  data lake. Payload is a plain, caller-supplied string
//                  (the caller serializes its own domain event/DTO before
//                  writing) rather than a typed object -- this keeps
//                  Enterprise.DataLake decoupled from every upstream
//                  domain project, matching Shared.SafetyData's
//                  established no-cross-repo-type-dependency precedent.
// ============================================================================
using Enterprise.DataLake.Abstractions.Enums;

namespace Enterprise.DataLake.Abstractions.Records;

/// <summary>A single immutable record in the enterprise data lake.</summary>
public sealed record DataLakeRecord(
    Guid RecordId,
    DataLakeRecordType RecordType,
    DataLakePartitionKey PartitionKey,
    Guid SourceId,
    string Payload,
    DateTime RecordedAtUtc);
