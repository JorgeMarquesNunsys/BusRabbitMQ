using System.Text;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusRabbitMQ.Shared.Helpers;

public sealed class LogHelper : IAdministradorLog, IDisposable
{
    private readonly IOptionsMonitor<ConfiguracionRegistroLog> _opciones;
    private readonly ILogger<LogHelper>? _logger;
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private bool _disposed;

    public LogHelper(IOptionsMonitor<ConfiguracionRegistroLog> opciones, ILogger<LogHelper>? logger = null)
    {
        _opciones = opciones ?? throw new ArgumentNullException(nameof(opciones));
        _logger = logger;
    }

    public async Task RegistrarEventoAsync(
        NivelLog tipoError,
        string? nombreClase,
        string? nombreMetodo,
        string mensaje,
        Exception? excepcion = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        var clase = string.IsNullOrWhiteSpace(nombreClase) ? "ClaseNoInformada" : nombreClase.Trim();
        var metodo = string.IsNullOrWhiteSpace(nombreMetodo) ? "MetodoNoInformado" : nombreMetodo.Trim();
        var directorio = ObtenerDirectorioSeguro();
        var nombreArchivo = $"{DateTime.UtcNow:yyyyMMdd}BusRabbitMQ.log";
        var rutaArchivo = Path.Combine(directorio, nombreArchivo);
        var linea = ConstruirLinea(tipoError, clase, metodo, mensaje, excepcion);

        var bloqueoObtenido = false;

        try
        {
            Directory.CreateDirectory(directorio);
            await _semaforo.WaitAsync(cancellationToken).ConfigureAwait(false);
            bloqueoObtenido = true;

            await using var stream = new FileStream(rutaArchivo, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            await using var writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteLineAsync(linea).ConfigureAwait(false);
        }
        catch (Exception errorEscritura)
        {
            _logger?.LogError(errorEscritura, "No fue posible registrar el log en disco.");
        }
        finally
        {
            if (bloqueoObtenido)
            {
                _semaforo.Release();
            }
        }
    }

    private string ObtenerDirectorioSeguro()
    {
        var configuracion = _opciones.CurrentValue ?? new ConfiguracionRegistroLog();
        var errores = configuracion.Validar();

        if (errores.Count > 0)
        {
            _logger?.LogWarning("Configuración de log inválida: {Errores}", string.Join("; ", errores));
        }

        return configuracion.ResolverDirectorio();
    }

    private string ConstruirLinea(NivelLog tipoError, string clase, string metodo, string mensaje, Exception? excepcion)
    {
        var builder = new StringBuilder();
        builder.Append(tipoError);
        builder.Append(":: Fecha:: ");
        builder.Append(DateTime.UtcNow.ToString("O"));
        builder.Append(":: @");
        builder.Append(clase);
        builder.Append(":: @");
        builder.Append(metodo);
        builder.Append(":: ");
        builder.Append(mensaje);

        if (excepcion is not null)
        {
            builder.Append(" | Excepcion: ");
            builder.Append(excepcion.Message);

            var incluirStackTrace = _opciones.CurrentValue?.IncluirStackTrace ?? true;
            if (incluirStackTrace && !string.IsNullOrWhiteSpace(excepcion.StackTrace))
            {
                builder.Append(" | StackTrace: ");
                builder.Append(excepcion.StackTrace);
            }
        }

        return builder.ToString();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _semaforo.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}