-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMiss_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [SafetyEvents].[NearMiss] by identity. Backs
--                  INearMissRepository.GetByIdAsync / NearMissDapperRepository.GetByIdAsync. Column
--                  list matches NearMissReadModel exactly. ETag is projected from the native
--                  ROWVERSION (CONVERT(NVARCHAR(34), RowVersion, 1)), matching the established
--                  concurrency-token convention. TRY/CATCH + THROW only (no AuditLog/ErrorLog
--                  writes), matching the established read-procedure convention. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMiss_GetById]
(
    @TenantId   UNIQUEIDENTIFIER,
    @NearMissId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            NearMissId,
            TenantId,
            SiteId,
            Title,
            Description,
            OccurredAtUtc,
            PotentialSeverity,
            HazardCategory,
            Status,
            ReportedByWorkerId,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[NearMiss]
        WHERE NearMissId = @NearMissId
          AND TenantId = @TenantId;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
