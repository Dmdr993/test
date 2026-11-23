using Citas.Api.Data;
using Citas.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CatalogosController(ICitaRepository repository) : ControllerBase
{
    [HttpGet("pacientes")]
    public async Task<ActionResult<IReadOnlyList<PacienteDto>>> Pacientes() =>
        Ok(await repository.ListarPacientesAsync());

    [HttpGet("cubiculos")]
    public async Task<ActionResult<IReadOnlyList<CubiculoDto>>> Cubiculos() =>
        Ok(await repository.ListarCubiculosAsync());

    [HttpPost("pacientes")]
    public async Task<ActionResult<PacienteDto>> CrearPaciente(CrearPacienteDto dto)
    {
        var paciente = await repository.CrearPacienteAsync(dto);
        return Created($"/api/pacientes/{paciente.Id}", paciente);
    }

    [HttpPost("cubiculos")]
    public async Task<ActionResult<CubiculoDto>> CrearCubiculo(CrearCubiculoDto dto)
    {
        var cubiculo = await repository.CrearCubiculoAsync(dto);
        return Created($"/api/cubiculos/{cubiculo.Id}", cubiculo);
    }
}

