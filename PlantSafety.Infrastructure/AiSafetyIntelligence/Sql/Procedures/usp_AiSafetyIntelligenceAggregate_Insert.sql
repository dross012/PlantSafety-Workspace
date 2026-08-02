-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiSafetyIntelligence
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AiSafetyIntelligenceAggregate_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new AiSafetyIntelligence aggregate root row. Backs
--                  IAiSafetyIntelligenceRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_Insert]
    @AiSafetyIntelligenceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @InsightType NVARCHAR(50),
    @Description NVARCHAR(2000),
    @ConfidenceScore DECIMAL(5,4),
    @SourceModelName NVARCHAR(200),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate]
        (
            AiSafetyIntelligenceId, TenantId, SiteId, Status, InsightType, Description,
            ConfidenceScore, SourceModelName, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @AiSafetyIntelligenceId, @TenantId, @SiteId, @Status, @InsightType, @Description,
            @ConfidenceScore, @SourceModelName, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AiSafetyIntelligenceAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; User=', SYSTEM_USER);
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
