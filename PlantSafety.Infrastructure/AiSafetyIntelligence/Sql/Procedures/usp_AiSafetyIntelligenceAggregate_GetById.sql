-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiSafetyIntelligence
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AiSafetyIntelligenceAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single AiSafetyIntelligence aggregate root row by identity,
--                  tenant-scoped from day one. Backs IAiSafetyIntelligenceRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetAiSafetyIntelligenceByIdQuery
--                  read path. Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Projects RowVersion as a string ETag.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_GetById]
    @AiSafetyIntelligenceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            AiSafetyIntelligenceId, TenantId, SiteId, Status, InsightType, Description,
            ConfidenceScore, SourceModelName,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate]
        WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
