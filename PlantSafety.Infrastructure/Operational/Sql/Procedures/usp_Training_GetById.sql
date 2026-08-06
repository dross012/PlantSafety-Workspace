-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Operational.Training
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Training_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [Operational].[TrainingAssignment] by identity.
--                  Backs ITrainingRepository.GetByIdAsync / TrainingDapperRepository.GetByIdAsync.
--                  Column list matches TrainingReadModel exactly. ETag is projected from the
--                  native ROWVERSION. TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the
--                  established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Training_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @RecordId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            RecordId,
            TenantId,
            WorkerId,
            TrainingCourseCode,
            TrainingCourseName,
            DeliveryMethod,
            Status,
            DueDateUtc,
            CompletedAtUtc,
            ExpiresAtUtc,
            Score,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Operational].[TrainingAssignment]
        WHERE RecordId = @RecordId
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
