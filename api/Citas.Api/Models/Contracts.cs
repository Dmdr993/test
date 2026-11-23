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
    [Range(1, int.MaxValue)] int PacienteId,
    [Range(1, int.MaxValue)] int CubiculoId,
    DateTime FechaHoraInicio);

public sealed record PacienteDto(int Id, string Nombres, string Dni);
public sealed record CubiculoDto(int Id, string Nombre, string Sede);

public sealed record CrearPacienteDto(
    [Required, StringLength(100)] string Nombres,
    [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos.")] string Dni,
    [StringLength(15)] string? Telefono);

public sealed record CrearCubiculoDto(
    [Required, StringLength(50)] string Nombre,
    [Required, StringLength(50)] string Sede);

