-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicAlertBroadcast_Insert.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PublicAlertBroadcast aggregate root row. Backs
--                  IPublicAlertBroadcastRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_Insert]
    @PublicAlertBroadcastId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @SeverityCode NVARCHAR(64),
    @ChannelCode NVARCHAR(64),
    @Headline NVARCHAR(200),
    @Message NVARCHAR(MAX),
    @TargetArea NVARCHAR(400),
    @IssuedAtUtc DATETIME2(3) = NULL,
    @IssuedBy UNIQUEIDENTIFIER = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PublicSafetyCommunication].[PublicAlertBroadcast]
        (
            PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
            TargetArea, IssuedAtUtc, IssuedBy, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PublicAlertBroadcastId, @TenantId, @SiteId, @Status, @IncidentId, @SeverityCode, @ChannelCode, @Headline, @Message,
            @TargetArea, @IssuedAtUtc, @IssuedBy, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PublicAlertBroadcast', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId, N'; User=', SYSTEM_USER);
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
