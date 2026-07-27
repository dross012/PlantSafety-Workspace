-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Moc_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IMocRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @MocId    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        MocId, TenantId, SiteId, Title, Description, ChangeType, RiskLevel, InitiatorId,
        ProposedStartUtc, ProposedEndUtc, Status, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Moc]
    WHERE MocId = @MocId AND TenantId = @TenantId AND IsDeleted = 0;
END
GO
