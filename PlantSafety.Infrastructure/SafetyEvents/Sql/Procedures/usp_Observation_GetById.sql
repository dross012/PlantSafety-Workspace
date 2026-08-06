-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Observation_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [SafetyEvents].[Observation] by identity. Backs
--                  IObservationRepository.GetByIdAsync / ObservationDapperRepository.GetByIdAsync.
--                  Column list matches ObservationReadModel exactly. ETag is projected from the
--                  native ROWVERSION. TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the
--                  established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Observation_GetById]
(
    @TenantId      UNIQUEIDENTIFIER,
    @ObservationId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            ObservationId,
            TenantId,
            SiteId,
            Title,
            Description,
            ObservationType,
            ObservedAtUtc,
            ObservedByWorkerId,
            Status,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[Observation]
        WHERE ObservationId = @ObservationId
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
