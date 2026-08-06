-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Rca_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [SafetyEvents].[Rca] by identity. Backs
--                  IRcaRepository.GetByIdAsync / RcaDapperRepository.GetByIdAsync. Column list
--                  matches RcaReadModel exactly. ETag is projected from the native ROWVERSION.
--                  TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the established
--                  read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Rca_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @RcaId    UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            RcaId,
            TenantId,
            LinkedEventId,
            LinkedEventType,
            Methodology,
            RootCauses,
            CorrectiveActions,
            LeadInvestigatorId,
            Status,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[Rca]
        WHERE RcaId = @RcaId
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
