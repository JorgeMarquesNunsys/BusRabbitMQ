using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusRabbitMQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ColasController : ControllerBase
{
    private readonly IServicioColaRabbit _servicioCola;

    public ColasController(IServicioColaRabbit servicioCola)
    {
        _servicioCola = servicioCola;
    }

    [HttpPost("send")]
    public async Task<IActionResult> EnviarAsync([FromBody] SolicitudOperacionCola solicitud, CancellationToken cancellationToken)
    {
        var resultado = await _servicioCola.EnviarAsync(solicitud, cancellationToken).ConfigureAwait(false);

        if (!resultado.EsExitoso)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }

    [HttpPost("publish")]
    public async Task<IActionResult> PublicarAsync([FromBody] SolicitudOperacionCola solicitud, CancellationToken cancellationToken)
    {
        var resultado = await _servicioCola.PublicarAsync(solicitud, cancellationToken).ConfigureAwait(false);

        if (!resultado.EsExitoso)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> SuscribirAsync([FromBody] SolicitudEstadoCola solicitud, CancellationToken cancellationToken)
    {
        var resultado = await _servicioCola.SuscribirAsync(solicitud, cancellationToken).ConfigureAwait(false);

        if (!resultado.EsExitoso)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }
}