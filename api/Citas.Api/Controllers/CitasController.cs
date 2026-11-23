using Citas.Api.Models;
using Citas.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Api.Controllers;

[ApiController]
[Route("api/citas")]
public sealed class CitasController(ICitaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> Listar(
        [FromQuery] int cubiculoId, [FromQuery] DateOnly fecha) =>
        Ok(await service.ListarAsync(cubiculoId, fecha));

    [HttpPost]
    public async Task<ActionResult<CitaDto>> Registrar(CrearCitaDto dto)
    {
        var cita = await service.RegistrarAsync(dto);
        return Created($"/api/citas/{cita.Id}", cita);
    }

    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id) =>
        await service.CancelarAsync(id) ? NoContent() : NotFound();
}

