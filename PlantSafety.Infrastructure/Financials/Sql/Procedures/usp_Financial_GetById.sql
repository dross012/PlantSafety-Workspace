-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Financial_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup backing FinancialsRepository.GetByIdAsync /
--                  GetFinancialsQueryHandler. Unlike the NearMiss/Observation/Rca findings
--                  elsewhere in this pass, this is NOT a missing table -- [Financials].[Financials]
--                  already exists (created by an earlier pass alongside usp_Financials_GetRecords)
--                  and its column set matches FinancialsReadModel exactly except for the PK column
--                  name (RecordId here vs FinancialId on the C# read model), which this proc
--                  aliases. Only the missing single-row lookup proc was absent. TRY/CATCH + THROW
--                  only (no AuditLog/ErrorLog writes) per the established read-procedure
--                  convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Financial_GetById]
(
    @TenantId    UNIQUEIDENTIFIER,
    @FinancialId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            RecordId AS FinancialId,
            Category,
            Amount,
            TenantId,
            CreatedAtUtc,
            CreatedBy
        FROM [Financials].[Financials]
        WHERE RecordId = @FinancialId
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
