using Citas.Api.Data;
using Citas.Api.Infrastructure;
using Citas.Api.Models;
using Citas.Api.Services;
using Xunit;

namespace Citas.Api.Tests;

public sealed class CitaServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 23, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Registrar_rechaza_fecha_pasada()
    {
        var service = new CitaService(new FakeRepository(), new FixedClock(Now));
        await Assert.ThrowsAsync<ValidationException>(() => service.RegistrarAsync(
            new CrearCitaDto(1, 1, new DateTime(2026, 11, 23, 9, 0, 0))));
    }

    [Fact]
    public async Task Registrar_rechaza_paciente_inexistente()
    {
        var repository = new FakeRepository { PacienteExiste = false };
        var service = new CitaService(repository, new FixedClock(Now));
        await Assert.ThrowsAsync<ValidationException>(() => service.RegistrarAsync(
            new CrearCitaDto(1, 1, new DateTime(2026, 11, 24, 9, 0, 0))));
    }

    [Fact]
    public async Task Registrar_devuelve_cita_valida()
    {
        var service = new CitaService(new FakeRepository(), new FixedClock(Now));
        var result = await service.RegistrarAsync(
            new CrearCitaDto(1, 1, new DateTime(2026, 11, 24, 9, 0, 0)));
        Assert.Equal(1, result.Id);
        Assert.Equal("PROGRAMADA", result.Estado);
    }

    [Fact]
    public async Task Cancelar_devuelve_falso_si_no_existe()
    {
        var service = new CitaService(new FakeRepository(), new FixedClock(Now));
        Assert.False(await service.CancelarAsync(99));
    }

    private sealed class FixedClock(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class FakeRepository : ICitaRepository
    {
        public bool PacienteExiste { get; init; } = true;
        public bool CubiculoExiste { get; init; } = true;
        public Task<bool> PacienteExisteAsync(int id) => Task.FromResult(PacienteExiste);
        public Task<bool> CubiculoExisteAsync(int id) => Task.FromResult(CubiculoExiste);
        public Task<CitaDto> RegistrarAsync(CrearCitaDto dto) => Task.FromResult(
            new CitaDto(1, dto.PacienteId, "Paciente", dto.CubiculoId, dto.FechaHoraInicio,
                dto.FechaHoraInicio.AddMinutes(30), "PROGRAMADA"));
        public Task<bool> CancelarAsync(int id) => Task.FromResult(id == 1);
        public Task<IReadOnlyList<CitaDto>> ListarAsync(int cubiculoId, DateOnly fecha) =>
            Task.FromResult<IReadOnlyList<CitaDto>>([]);
        public Task<IReadOnlyList<PacienteDto>> ListarPacientesAsync() =>
            Task.FromResult<IReadOnlyList<PacienteDto>>([]);
        public Task<IReadOnlyList<CubiculoDto>> ListarCubiculosAsync() =>
            Task.FromResult<IReadOnlyList<CubiculoDto>>([]);
        public Task<PacienteDto> CrearPacienteAsync(CrearPacienteDto dto) =>
            Task.FromResult(new PacienteDto(1, dto.Nombres, dto.Dni));
        public Task<CubiculoDto> CrearCubiculoAsync(CrearCubiculoDto dto) =>
            Task.FromResult(new CubiculoDto(1, dto.Nombre, dto.Sede));
    }
}

