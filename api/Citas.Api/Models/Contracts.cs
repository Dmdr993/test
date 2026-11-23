using System.ComponentModel.DataAnnotations;

namespace Citas.Api.Models;

public sealed record CitaDto(
    int Id,
    int PacienteId,
    string PacienteNombre,
    int CubiculoId,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    string Estado);

public sealed record CrearCitaDto(
    [property: Range(1, int.MaxValue)] int PacienteId,
    [property: Range(1, int.MaxValue)] int CubiculoId,
    DateTime FechaHoraInicio);

public sealed record PacienteDto(int Id, string Nombres, string Dni);
public sealed record CubiculoDto(int Id, string Nombre, string Sede);

