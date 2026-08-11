-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_StopWorkEvent_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyCulture].[StopWorkEvent], backing
--                  IStopWorkEventRepository.AddAsync. Standard error envelope: TRY/CATCH
--                  with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_StopWorkEvent_Create]
    @TenantId                     UNIQUEIDENTIFIER,
    @CorrelationId                UNIQUEIDENTIFIER,
    @ActorId                      UNIQUEIDENTIFIER,
    @NowUtc                       DATETIME2(3),
    @StopWorkEventId              UNIQUEIDENTIFIER,
    @SiteId                       UNIQUEIDENTIFIER = NULL,
    @Status                       NVARCHAR(20),
    @WorkArea                     NVARCHAR(200),
    @HazardDescription            NVARCHAR(2000),
    @InitiatedByActorKind         NVARCHAR(20),
    @InitiatedByUserId            UNIQUEIDENTIFIER = NULL,
    @WasCelebrated                BIT,
    @RelatedPermitId              UNIQUEIDENTIFIER = NULL,
    @RelatedChecklistCompletionId UNIQUEIDENTIFIER = NULL,
    @CreatedAtUtc                 DATETIME2(3),
    @CreatedBy                    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyCulture].[StopWorkEvent]
            ([StopWorkEventId], [TenantId], [SiteId], [Status], [WorkArea], [HazardDescription],
             [InitiatedByActorKind], [InitiatedByUserId], [WasCelebrated],
             [RelatedPermitId], [RelatedChecklistCompletionId], [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@StopWorkEventId, @TenantId, @SiteId, @Status, @WorkArea, @HazardDescription,
             @InitiatedByActorKind, @InitiatedByUserId, @WasCelebrated,
             @RelatedPermitId, @RelatedChecklistCompletionId, @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'StopWorkEvent', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'StopWorkEventId=', @StopWorkEventId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'StopWorkEventId=', @StopWorkEventId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
