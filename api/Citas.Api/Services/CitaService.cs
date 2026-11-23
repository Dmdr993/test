using Citas.Api.Data;
using Citas.Api.Infrastructure;
using Citas.Api.Models;

namespace Citas.Api.Services;

public interface ICitaService
{
    Task<IReadOnlyList<CitaDto>> ListarAsync(int cubiculoId, DateOnly fecha);
    Task<CitaDto> RegistrarAsync(CrearCitaDto dto);
    Task<bool> CancelarAsync(int id);
}

public sealed class CitaService(ICitaRepository repository, TimeProvider clock) : ICitaService
{
    public async Task<IReadOnlyList<CitaDto>> ListarAsync(int cubiculoId, DateOnly fecha)
    {
        if (cubiculoId <= 0) throw new ValidationException("El cubículo es obligatorio.");
        return await repository.ListarAsync(cubiculoId, fecha);
    }

    public async Task<CitaDto> RegistrarAsync(CrearCitaDto dto)
    {
        var ahoraLocal = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time")).DateTime;
        if (dto.FechaHoraInicio <= ahoraLocal)
            throw new ValidationException("La fecha y hora deben ser futuras.");
        if (!await repository.PacienteExisteAsync(dto.PacienteId))
            throw new ValidationException("El paciente no existe.");
        if (!await repository.CubiculoExisteAsync(dto.CubiculoId))
            throw new ValidationException("El cubículo no existe.");
        return await repository.RegistrarAsync(dto);
    }

    public Task<bool> CancelarAsync(int id)
    {
        if (id <= 0) throw new ValidationException("El identificador no es válido.");
        return repository.CancelarAsync(id);
    }
}

