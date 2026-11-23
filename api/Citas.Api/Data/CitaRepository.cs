using System.Data;
using Citas.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Citas.Api.Data;

public interface ICitaRepository
{
    Task<IReadOnlyList<CitaDto>> ListarAsync(int cubiculoId, DateOnly fecha);
    Task<CitaDto> RegistrarAsync(CrearCitaDto dto);
    Task<bool> PacienteExisteAsync(int id);
    Task<bool> CubiculoExisteAsync(int id);
    Task<bool> CancelarAsync(int id);
    Task<IReadOnlyList<PacienteDto>> ListarPacientesAsync();
    Task<IReadOnlyList<CubiculoDto>> ListarCubiculosAsync();
}

public sealed class SqlCitaRepository(IConfiguration configuration) : ICitaRepository
{
    private SqlConnection CrearConexion() => new(configuration.GetConnectionString("Clinica")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Clinica."));

    public async Task<IReadOnlyList<CitaDto>> ListarAsync(int cubiculoId, DateOnly fecha)
    {
        const string sql = """
            SELECT c.Id, c.PacienteId, p.Nombres AS PacienteNombre, c.CubiculoId,
                   c.FechaHoraInicio, c.FechaHoraFin, c.Estado
            FROM dbo.Cita c
            INNER JOIN dbo.Paciente p ON p.Id = c.PacienteId
            WHERE c.CubiculoId = @cubiculoId
              AND c.FechaHoraInicio >= @inicio
              AND c.FechaHoraInicio < @fin
            ORDER BY c.FechaHoraInicio;
            """;
        await using var connection = CrearConexion();
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        return (await connection.QueryAsync<CitaDto>(sql, new { cubiculoId, inicio, fin = inicio.AddDays(1) })).AsList();
    }

    public async Task<CitaDto> RegistrarAsync(CrearCitaDto dto)
    {
        await using var connection = CrearConexion();
        return await connection.QuerySingleAsync<CitaDto>(
            "dbo.usp_Cita_Registrar",
            new { dto.PacienteId, dto.CubiculoId, dto.FechaHoraInicio },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> PacienteExisteAsync(int id) =>
        await ExisteAsync("SELECT COUNT(1) FROM dbo.Paciente WHERE Id = @id", id);

    public async Task<bool> CubiculoExisteAsync(int id) =>
        await ExisteAsync("SELECT COUNT(1) FROM dbo.Cubiculo WHERE Id = @id", id);

    private async Task<bool> ExisteAsync(string sql, int id)
    {
        await using var connection = CrearConexion();
        return await connection.ExecuteScalarAsync<int>(sql, new { id }) > 0;
    }

    public async Task<bool> CancelarAsync(int id)
    {
        const string sql = """
            UPDATE dbo.Cita SET Estado = 'CANCELADA'
            WHERE Id = @id AND Estado <> 'CANCELADA';
            IF @@ROWCOUNT = 0
                SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Cita WHERE Id = @id) THEN 1 ELSE 0 END;
            ELSE SELECT 1;
            """;
        await using var connection = CrearConexion();
        return await connection.ExecuteScalarAsync<int>(sql, new { id }) == 1;
    }

    public async Task<IReadOnlyList<PacienteDto>> ListarPacientesAsync()
    {
        await using var connection = CrearConexion();
        return (await connection.QueryAsync<PacienteDto>(
            "SELECT Id, Nombres, Dni FROM dbo.Paciente ORDER BY Nombres")).AsList();
    }

    public async Task<IReadOnlyList<CubiculoDto>> ListarCubiculosAsync()
    {
        await using var connection = CrearConexion();
        return (await connection.QueryAsync<CubiculoDto>(
            "SELECT Id, Nombre, Sede FROM dbo.Cubiculo ORDER BY Sede, Nombre")).AsList();
    }
}

