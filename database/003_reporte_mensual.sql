USE ClinicaLaser;
GO

DECLARE @InicioMes DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @FinMes DATE = DATEADD(MONTH, 1, @InicioMes);

SELECT cu.Sede,
       CONVERT(DATE, ci.FechaHoraInicio) AS Dia,
       SUM(CASE WHEN ci.Estado = 'ATENDIDA' THEN 1 ELSE 0 END) AS TotalAtendidas,
       SUM(CASE WHEN ci.Estado = 'CANCELADA' THEN 1 ELSE 0 END) AS TotalCanceladas
FROM dbo.Cita ci
INNER JOIN dbo.Cubiculo cu ON cu.Id = ci.CubiculoId
WHERE ci.FechaHoraInicio >= @InicioMes
  AND ci.FechaHoraInicio < @FinMes
  AND ci.Estado IN ('ATENDIDA', 'CANCELADA')
GROUP BY cu.Sede, CONVERT(DATE, ci.FechaHoraInicio)
ORDER BY cu.Sede, Dia;

