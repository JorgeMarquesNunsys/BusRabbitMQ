using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusRabbitMQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ColasController : ControllerBase
{
    private readonly IServicioColaRabbit _servicioCola;
    private readonly IContextoConexionRabbit _contextoConexion;

    public ColasController(IServicioColaRabbit servicioCola, IContextoConexionRabbit contextoConexion)
    {
        _servicioCola = servicioCola;
        _contextoConexion = contextoConexion;
    }

    [HttpGet("connection")]
    public ActionResult<ConfiguracionConexionRabbit> ObtenerConexionActual()
    {
        var configuracion = _contextoConexion.ObtenerConfiguracionActual();
        return Ok(configuracion);
    }

    [HttpPost("connection")]
    public async Task<IActionResult> ActualizarConexionAsync(
        [FromBody] ConfiguracionConexionRabbit configuracion,
        CancellationToken cancellationToken)
    {
        var resultado = await _contextoConexion.ActualizarConfiguracionAsync(configuracion, cancellationToken)
            .ConfigureAwait(false);

        if (!resultado.EsExitoso)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }

    [HttpPost("connection/default")]
    public async Task<IActionResult> RestablecerConexionPredeterminadaAsync(CancellationToken cancellationToken)
    {
        var resultado = await _contextoConexion.RestablecerConfiguracionPredeterminadaAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!resultado.EsExitoso)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
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