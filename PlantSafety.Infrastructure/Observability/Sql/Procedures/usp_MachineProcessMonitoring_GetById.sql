-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_MachineProcessMonitoring_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single MachineProcessMonitoring row by identity. Backs IMachineProcessMonitoringRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_MachineProcessMonitoring_GetById]
    @MachineProcessMonitoringId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT MachineProcessMonitoringId, TenantId, SiteId, Status, MachineId, SensorType, Reading, ThresholdBreached, AcknowledgedBy, AnomalyDescription, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[MachineProcessMonitoring]
    WHERE MachineProcessMonitoringId = @MachineProcessMonitoringId AND IsDeleted = 0;
END
GO
