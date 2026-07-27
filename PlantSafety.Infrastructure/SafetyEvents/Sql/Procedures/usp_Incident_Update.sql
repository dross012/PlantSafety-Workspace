-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Incident_Update.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates mutable fields on an existing [SafetyEvents].[Incident] row. Backs
--                  IIncidentRepository.UpdateAsync / IncidentDapperRepository.UpdateAsync.
--                  Per the IIncidentRepository contract ("Only fields with non-null values are
--                  updated"), Title/Description/Severity/Location/AssignedInvestigatorId use
--                  ISNULL/COALESCE-against-current-value so a NULL argument leaves the existing
--                  value untouched rather than clearing it. Optimistic concurrency is enforced
--                  by matching @ETag against the row's native ROWVERSION (projected the same way
--                  as ProcessManagement.usp_Process_GetById: CONVERT(NVARCHAR(34), RowVersion,
--                  1)); a stale ETag, wrong tenant, missing row, or soft-deleted row all
--                  naturally produce zero rows affected, which IncidentDapperRepository.
--                  UpdateAsync surfaces as ConcurrencyConflictException -- no separate
--                  IF NOT EXISTS/THROW existence check is used here (unlike
--                  usp_Accident_Update/usp_Risk_Update) so that both cases route through the
--                  same rows-affected-based concurrency contract the caller expects.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the pattern already established by
--                  usp_Accident_Update. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_Update]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @IncidentId             UNIQUEIDENTIFIER,
    @Title                  NVARCHAR(200)  = NULL,
    @Description            NVARCHAR(4000) = NULL,
    @Severity               NVARCHAR(30)   = NULL,
    @Location               NVARCHAR(500)  = NULL,
    @AssignedInvestigatorId UNIQUEIDENTIFIER = NULL,
    @ETag                   NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SafetyEvents].[Incident]
        SET
            Title = ISNULL(@Title, Title),
            Description = ISNULL(@Description, Description),
            Severity = ISNULL(@Severity, Severity),
            Location = ISNULL(@Location, Location),
            AssignedInvestigatorId = COALESCE(@AssignedInvestigatorId, AssignedInvestigatorId),
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc = @NowUtc
        WHERE IncidentId = @IncidentId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), RowVersion, 1) = @ETag;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Incident', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
