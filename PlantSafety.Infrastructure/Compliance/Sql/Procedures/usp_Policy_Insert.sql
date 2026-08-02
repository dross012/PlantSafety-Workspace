-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Policy
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Policy_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Policy aggregate root row into [Compliance].[PolicyAggregate].
--                  Backs IPolicyRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging per the
--                  canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Policy_Insert]
    @PolicyId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @PolicyTitle NVARCHAR(400),
    @PolicyVersion INT,
    @Category NVARCHAR(100),
    @OwnerId UNIQUEIDENTIFIER,
    @EffectiveAtUtc DATETIME2(3) = NULL,
    @AcknowledgmentRequired BIT,
    @AcknowledgmentCount INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[PolicyAggregate]
        (
            PolicyId, TenantId, SiteId, Status, PolicyTitle, PolicyVersion, Category, OwnerId, EffectiveAtUtc, AcknowledgmentRequired, AcknowledgmentCount, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PolicyId, @TenantId, @SiteId, @Status, @PolicyTitle, @PolicyVersion, @Category, @OwnerId, @EffectiveAtUtc, @AcknowledgmentRequired, @AcknowledgmentCount, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PolicyAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PolicyId=', @PolicyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; User=', SYSTEM_USER);
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
