using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusRabbitMQ.API.Controllers;

/// <summary>
/// Controlador encargado de administrar las configuraciones y operaciones
/// disponibles sobre las colas expuestas vía RabbitMQ.
/// </summary>
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

    /// <summary>
    /// Obtiene la configuración de conexión actualmente activa.
    /// </summary>
    /// <returns>Configuración de conexión vigente en el contexto.</returns>
    [HttpGet("connection")]
    public ActionResult<ConfiguracionConexionRabbit> ObtenerConexionActual()
    {
        var configuracion = _contextoConexion.ObtenerConfiguracionActual();
        return Ok(configuracion);
    }

    /// <summary>
    /// Actualiza la conexión activa utilizando los parámetros proporcionados.
    /// </summary>
    /// <param name="configuracion">Configuración completa que reemplazará la conexión actual.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="ResultadoOperacionCola{T}"/> indicando éxito o errores de validación.
    /// </returns>
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

    /// <summary>
    /// Restablece la conexión activa a la configuración predeterminada definida en appsettings.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="ResultadoOperacionCola{T}"/> con la configuración restaurada.
    /// </returns>
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

    /// <summary>
    /// Envía un mensaje directo a la cola especificada y valida su disponibilidad inmediata.
    /// </summary>
    /// <param name="solicitud">Datos del mensaje y la cola destino.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
    /// <returns>
    /// Resultado de la operación, incluyendo errores cuando no existen suscriptores activos.
    /// </returns>
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

    /// <summary>
    /// Publica un mensaje en modo fan-out para que todos los suscriptores lo reciban.
    /// </summary>
    /// <param name="solicitud">Información del mensaje a publicar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
    /// <returns>Resultado de la publicación, con errores cuando la operación falla.</returns>
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

    /// <summary>
    /// Consulta el estado de la cola objetivo, incluyendo métricas y contenido resumido.
    /// </summary>
    /// <param name="solicitud">Solicitud con la cola a inspeccionar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
    /// <returns>Detalle del estado actual de la cola solicitada.</returns>
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