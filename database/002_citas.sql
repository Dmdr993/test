USE ClinicaLaser;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Cita_Cubiculo_FechaHoraInicio'
      AND object_id = OBJECT_ID(N'dbo.Cita')
)
BEGIN
    CREATE INDEX IX_Cita_Cubiculo_FechaHoraInicio
        ON dbo.Cita (CubiculoId, FechaHoraInicio)
        INCLUDE (FechaHoraFin, Estado, PacienteId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Cita_Registrar
    @PacienteId INT,
    @CubiculoId INT,
    @FechaHoraInicio DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @FechaHoraFin DATETIME2 = DATEADD(MINUTE, 30, @FechaHoraInicio);

    BEGIN TRY
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM dbo.Cita WITH (UPDLOCK, HOLDLOCK, INDEX(IX_Cita_Cubiculo_FechaHoraInicio))
            WHERE CubiculoId = @CubiculoId
              AND Estado <> 'CANCELADA'
              AND FechaHoraInicio < @FechaHoraFin
              AND FechaHoraFin > @FechaHoraInicio
        )
            THROW 51001, 'El cubículo ya tiene una cita que cruza este horario.', 1;

        INSERT dbo.Cita (PacienteId, CubiculoId, FechaHoraInicio, FechaHoraFin)
        VALUES (@PacienteId, @CubiculoId, @FechaHoraInicio, @FechaHoraFin);

        DECLARE @CitaId INT = CONVERT(INT, SCOPE_IDENTITY());
        COMMIT TRANSACTION;

        SELECT c.Id, c.PacienteId, p.Nombres AS PacienteNombre, c.CubiculoId,
               c.FechaHoraInicio, c.FechaHoraFin, c.Estado
        FROM dbo.Cita c
        INNER JOIN dbo.Paciente p ON p.Id = c.PacienteId
        WHERE c.Id = @CitaId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

