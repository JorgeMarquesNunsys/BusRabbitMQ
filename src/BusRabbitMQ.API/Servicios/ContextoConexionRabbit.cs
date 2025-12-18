using System.Threading;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.Extensions.Options;

namespace BusRabbitMQ.API.Servicios;

public sealed class ContextoConexionRabbit : IContextoConexionRabbit, IDisposable
{
    private readonly IOptionsMonitor<ConfiguracionConexionRabbit> _opciones;
    private readonly IAdministradorLog _administradorLog;
    private readonly ReaderWriterLockSlim _bloqueo = new(LockRecursionPolicy.NoRecursion);
    private ConfiguracionConexionRabbit _configuracionActual;
    private ConfiguracionConexionRabbit _configuracionPredeterminada;
    private readonly IDisposable? _suscripcionCambios;
    private bool _disposed;

    public ContextoConexionRabbit(
        IOptionsMonitor<ConfiguracionConexionRabbit> opciones,
        IAdministradorLog administradorLog)
    {
        _opciones = opciones ?? throw new ArgumentNullException(nameof(opciones));
        _administradorLog = administradorLog ?? throw new ArgumentNullException(nameof(administradorLog));

        var configuracionInicial = Normalizar(_opciones.CurrentValue);
        _configuracionPredeterminada = configuracionInicial;
        _configuracionActual = configuracionInicial;

        _suscripcionCambios = _opciones.OnChange(config => ActualizarConfiguracionPredeterminadaInterna(Normalizar(config)));
    }

    public ConfiguracionConexionRabbit ObtenerConfiguracionActual()
    {
        _bloqueo.EnterReadLock();
        try
        {
            return _configuracionActual;
        }
        finally
        {
            _bloqueo.ExitReadLock();
        }
    }

    public async Task<ResultadoOperacionCola<ConfiguracionConexionRabbit>> ActualizarConfiguracionAsync(
        ConfiguracionConexionRabbit nuevaConfiguracion,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (nuevaConfiguracion is null)
        {
            return ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearFallo(
                new[] { "La configuración de conexión es obligatoria." });
        }

        var errores = nuevaConfiguracion.Validar();
        if (errores.Count > 0)
        {
            return ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearFallo(errores, "La configuración recibida es inválida.");
        }

        ActualizarConfiguracionActualInterna(nuevaConfiguracion);

        await _administradorLog.RegistrarEventoAsync(
                NivelLog.Informacion,
                nameof(ContextoConexionRabbit),
                nameof(ActualizarConfiguracionAsync),
                $"Conexión activa actualizada a la cola '{nuevaConfiguracion.NombreColaPorDefecto}'.",
                null,
                cancellationToken)
            .ConfigureAwait(false);

        return ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearExito(
            ObtenerConfiguracionActual(),
            "Conexión actualizada correctamente.");
    }

    public async Task<ResultadoOperacionCola<ConfiguracionConexionRabbit>> RestablecerConfiguracionPredeterminadaAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configuracionPredeterminada = ObtenerConfiguracionPredeterminada();

        ActualizarConfiguracionActualInterna(configuracionPredeterminada);

        await _administradorLog.RegistrarEventoAsync(
                NivelLog.Informacion,
                nameof(ContextoConexionRabbit),
                nameof(RestablecerConfiguracionPredeterminadaAsync),
                $"Conexión activa restablecida a la configuración predeterminada '{configuracionPredeterminada.NombreColaPorDefecto}'.",
                null,
                cancellationToken)
            .ConfigureAwait(false);

        return ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearExito(
            configuracionPredeterminada,
            "Conexión restablecida correctamente a la configuración predeterminada.");
    }

    private ConfiguracionConexionRabbit ObtenerConfiguracionPredeterminada()
    {
        _bloqueo.EnterReadLock();
        try
        {
            return _configuracionPredeterminada;
        }
        finally
        {
            _bloqueo.ExitReadLock();
        }
    }

    private void ActualizarConfiguracionActualInterna(ConfiguracionConexionRabbit configuracion)
    {
        _bloqueo.EnterWriteLock();
        try
        {
            _configuracionActual = configuracion;
        }
        finally
        {
            _bloqueo.ExitWriteLock();
        }
    }

    private void ActualizarConfiguracionPredeterminadaInterna(ConfiguracionConexionRabbit configuracion)
    {
        _bloqueo.EnterWriteLock();
        try
        {
            _configuracionPredeterminada = configuracion;
        }
        finally
        {
            _bloqueo.ExitWriteLock();
        }
    }

    private static ConfiguracionConexionRabbit Normalizar(ConfiguracionConexionRabbit? configuracion)
    {
        return configuracion ?? new ConfiguracionConexionRabbit();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _suscripcionCambios?.Dispose();
        _bloqueo.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}