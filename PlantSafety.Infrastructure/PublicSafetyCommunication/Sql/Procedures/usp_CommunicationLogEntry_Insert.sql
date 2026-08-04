-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CommunicationLogEntry_Insert.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new, immutable CommunicationLogEntry aggregate root row. Backs
--                  ICommunicationLogEntryRepository.AddAsync. There is no corresponding Update
--                  procedure -- the table is append-only, consistent with the aggregate's
--                  data-governance immutability policy. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_Insert]
    @CommunicationLogEntryId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @RelatedIncidentNotificationId UNIQUEIDENTIFIER = NULL,
    @RelatedPublicAlertBroadcastId UNIQUEIDENTIFIER = NULL,
    @Direction NVARCHAR(32),
    @Medium NVARCHAR(32),
    @CounterpartyName NVARCHAR(400),
    @Summary NVARCHAR(MAX),
    @OccurredAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PublicSafetyCommunication].[CommunicationLogEntry]
        (
            CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
            Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @CommunicationLogEntryId, @TenantId, @SiteId, @IncidentId, @RelatedIncidentNotificationId, @RelatedPublicAlertBroadcastId,
            @Direction, @Medium, @CounterpartyName, @Summary, @OccurredAtUtc, @CreatedAtUtc, @CreatedBy
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CommunicationLogEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CommunicationLogEntryId=', @CommunicationLogEntryId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CommunicationLogEntryId=', @CommunicationLogEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
