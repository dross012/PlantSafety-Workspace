-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Financial_Insert.sql
--  Created:        2026-06-17
--  Description:    Insert stored procedure for the Financials module.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Financial_Insert]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @Category       NVARCHAR(200),
    @Amount         DECIMAL(18,2),
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        SET @Id = NEWID();
        INSERT INTO [Financials].[Transactions] (Id, TenantId, Category, Amount, CreatedBy, CreatedAt)
        VALUES (@Id, @TenantId, @Category, @Amount, @ActorId, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
