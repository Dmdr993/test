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
}

