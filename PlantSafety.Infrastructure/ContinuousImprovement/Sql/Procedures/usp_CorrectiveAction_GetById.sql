-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CorrectiveAction_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [ContinuousImprovement].[CorrectiveAction] by
--                  identity. Backs ICorrectiveActionRepository.GetByIdAsync /
--                  CorrectiveActionDapperRepository.GetByIdAsync. Parameter names/types and the
--                  column list mirror the StoredProcedureBuilder Add* calls and
--                  CorrectiveActionReadModel contract exactly. ETag is projected from the native
--                  ROWVERSION (CONVERT(NVARCHAR(34), RowVersion, 1)), matching the concurrency-
--                  token convention established by usp_Process_GetById / usp_IncidentRead_GetById.
--                  Soft-delete filtering (IsDeleted = 0) applied per canonical pattern. TRY/CATCH +
--                  THROW only (no AuditLog/ErrorLog writes), matching the established read-
--                  procedure convention (see usp_IncidentRead_GetById / usp_Process_GetById) --
--                  read procedures do not write audit or error-log entries. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @CaId     UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            CaId,
            TenantId,
            SourceEventId,
            SourceEventType,
            Title,
            Description,
            ActionType,
            Priority,
            AssignedToWorkerId,
            DueAtUtc,
            Status,
            CompletedAtUtc,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [ContinuousImprovement].[CorrectiveAction]
        WHERE CaId = @CaId
          AND TenantId = @TenantId
          AND IsDeleted = 0;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
