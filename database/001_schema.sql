IF DB_ID(N'ClinicaLaser') IS NULL
    CREATE DATABASE ClinicaLaser;
GO

USE ClinicaLaser;
GO

IF OBJECT_ID(N'dbo.Paciente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Paciente (
        Id INT IDENTITY PRIMARY KEY,
        Nombres NVARCHAR(100) NOT NULL,
        Dni CHAR(8) NOT NULL UNIQUE,
        Telefono VARCHAR(15) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Cubiculo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cubiculo (
        Id INT IDENTITY PRIMARY KEY,
        Nombre NVARCHAR(50) NOT NULL,
        Sede NVARCHAR(50) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Cita', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cita (
        Id INT IDENTITY PRIMARY KEY,
        PacienteId INT NOT NULL REFERENCES dbo.Paciente(Id),
        CubiculoId INT NOT NULL REFERENCES dbo.Cubiculo(Id),
        FechaHoraInicio DATETIME2 NOT NULL,
        FechaHoraFin DATETIME2 NOT NULL,
        Estado VARCHAR(20) NOT NULL CONSTRAINT DF_Cita_Estado DEFAULT 'PROGRAMADA',
        CreadoEn DATETIME2 NOT NULL CONSTRAINT DF_Cita_CreadoEn DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Cita_Estado CHECK (Estado IN ('PROGRAMADA', 'ATENDIDA', 'CANCELADA')),
        CONSTRAINT CK_Cita_Rango CHECK (FechaHoraFin > FechaHoraInicio)
    );
END;
GO

