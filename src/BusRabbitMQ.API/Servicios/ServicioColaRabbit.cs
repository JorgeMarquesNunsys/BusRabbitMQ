using System.Text;
using System.Text.Json;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace BusRabbitMQ.API.Servicios;

public sealed class ServicioColaRabbit : IServicioColaRabbit
{
    private readonly IContextoConexionRabbit _contextoConexion;
    private readonly IAdministradorLog _administradorLog;
    private readonly ILogger<ServicioColaRabbit>? _logger;

    public ServicioColaRabbit(
        IContextoConexionRabbit contextoConexion,
        IAdministradorLog administradorLog,
        ILogger<ServicioColaRabbit>? logger = null)
    {
        _contextoConexion = contextoConexion ?? throw new ArgumentNullException(nameof(contextoConexion));
        _administradorLog = administradorLog ?? throw new ArgumentNullException(nameof(administradorLog));
        _logger = logger;
    }

    public async Task<ResultadoOperacionCola<string>> EnviarAsync(SolicitudOperacionCola solicitud, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var errores = ValidarSolicitudOperacion(solicitud, TipoOperacionCola.Enviar);
        if (errores.Count > 0)
        {
            return ResultadoOperacionCola<string>.CrearFallo(errores, "La solicitud de envío es inválida.");
        }

        try
        {
            var configuracion = ObtenerConfiguracionEfectiva();
            var nombreCola = ResolverNombreCola(solicitud.NombreCola, configuracion);

            using var conexion = CrearConexion(configuracion);
            using var canal = conexion.CreateModel();
            ConfigurarCalidadServicio(canal, configuracion);
            var declaracion = DeclararCola(canal, nombreCola, configuracion);

            if (declaracion.ConsumerCount == 0 && !solicitud.PermitirPublicarSinSuscriptores)
            {
                return ResultadoOperacionCola<string>.CrearFallo(
                    new[] { "No existe ningún suscriptor escuchando la cola solicitada." },
                    "No fue posible realizar el envío.");
            }

            var cuerpo = CodificarContenido(solicitud.Contenido);
            var propiedades = CrearPropiedadesMensaje(canal, solicitud, configuracion);

            canal.BasicPublish(
                exchange: string.Empty,
                routingKey: nombreCola,
                mandatory: true,
                basicProperties: propiedades,
                body: cuerpo);

            var mensajeObtenido = canal.BasicGet(nombreCola, autoAck: false);
            if (mensajeObtenido is null)
            {
                return ResultadoOperacionCola<string>.CrearFallo(new[] { "El mensaje no se encuentra disponible en la cola." });
            }

            var contenido = DecodificarMensaje(mensajeObtenido.Body);
            canal.BasicNack(mensajeObtenido.DeliveryTag, multiple: false, requeue: true);

            return ResultadoOperacionCola<string>.CrearExito(contenido, "Mensaje enviado y disponible para los suscriptores.");
        }
        catch (Exception ex)
        {
            await RegistrarErrorAsync(nameof(ServicioColaRabbit), nameof(EnviarAsync), ex, cancellationToken)
                .ConfigureAwait(false);

            return ResultadoOperacionCola<string>.CrearFallo(
                new[] { "Ocurrió un error inesperado durante el envío del mensaje." });
        }
    }

    public async Task<ResultadoOperacionCola<bool>> PublicarAsync(SolicitudOperacionCola solicitud, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var errores = ValidarSolicitudOperacion(solicitud, TipoOperacionCola.Publicar);
        if (errores.Count > 0)
        {
            return ResultadoOperacionCola<bool>.CrearFallo(errores, "La solicitud de publicación es inválida.");
        }

        try
        {
            var configuracion = ObtenerConfiguracionEfectiva();
            var nombreCola = ResolverNombreCola(solicitud.NombreCola, configuracion);

            using var conexion = CrearConexion(configuracion);
            using var canal = conexion.CreateModel();
            ConfigurarCalidadServicio(canal, configuracion);
            DeclararCola(canal, nombreCola, configuracion);

            var cuerpo = CodificarContenido(solicitud.Contenido);
            var propiedades = CrearPropiedadesMensaje(canal, solicitud, configuracion);

            canal.BasicPublish(
                exchange: string.Empty,
                routingKey: nombreCola,
                mandatory: true,
                basicProperties: propiedades,
                body: cuerpo);

            return ResultadoOperacionCola<bool>.CrearExito(true, "Mensaje publicado en la cola.");
        }
        catch (Exception ex)
        {
            await RegistrarErrorAsync(nameof(ServicioColaRabbit), nameof(PublicarAsync), ex, cancellationToken)
                .ConfigureAwait(false);

            return ResultadoOperacionCola<bool>.CrearFallo(
                new[] { "Ocurrió un error inesperado durante la publicación del mensaje." });
        }
    }

    public async Task<ResultadoOperacionCola<EstadoColaDetalle>> SuscribirAsync(
        SolicitudEstadoCola solicitud,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var errores = ValidarSolicitudEstado(solicitud);
        if (errores.Count > 0)
        {
            return ResultadoOperacionCola<EstadoColaDetalle>.CrearFallo(errores, "La solicitud de suscripción es inválida.");
        }

        try
        {
            var configuracion = ObtenerConfiguracionEfectiva();
            var nombreCola = ResolverNombreCola(solicitud.NombreCola, configuracion);

            using var conexion = CrearConexion(configuracion);
            using var canal = conexion.CreateModel();
            ConfigurarCalidadServicio(canal, configuracion);
            var declaracion = DeclararCola(canal, nombreCola, configuracion);

            var totalMensajes = declaracion.MessageCount;
            var consumidores = declaracion.ConsumerCount;
            var resumenMensajes = new List<string>();

            if (solicitud.IncluirContenido && totalMensajes > 0)
            {
                var limite = Math.Min((uint)solicitud.MaximoMensajes, totalMensajes);
                for (var i = 0; i < limite; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var mensaje = canal.BasicGet(nombreCola, autoAck: false);
                    if (mensaje is null)
                    {
                        break;
                    }

                    resumenMensajes.Add(DecodificarMensaje(mensaje.Body));
                    canal.BasicNack(mensaje.DeliveryTag, multiple: false, requeue: true);
                }
            }

            var estado = new EstadoColaDetalle
            {
                NombreCola = nombreCola,
                TotalMensajesListos = totalMensajes,
                TotalMensajesEnProcesamiento = consumidores,
                ResumenMensajes = resumenMensajes
            };

            return ResultadoOperacionCola<EstadoColaDetalle>.CrearExito(estado, "Estado de la cola recuperado correctamente.");
        }
        catch (Exception ex)
        {
            await RegistrarErrorAsync(nameof(ServicioColaRabbit), nameof(SuscribirAsync), ex, cancellationToken)
                .ConfigureAwait(false);

            return ResultadoOperacionCola<EstadoColaDetalle>.CrearFallo(
                new[] { "Ocurrió un error inesperado durante la suscripción a la cola." });
        }
    }

    private IReadOnlyCollection<string> ValidarSolicitudOperacion(SolicitudOperacionCola? solicitud, TipoOperacionCola tipoOperacion)
    {
        if (solicitud is null)
        {
            return new[] { "La solicitud es obligatoria." };
        }

        return solicitud.Validar(tipoOperacion);
    }

    private IReadOnlyCollection<string> ValidarSolicitudEstado(SolicitudEstadoCola? solicitud)
    {
        if (solicitud is null)
        {
            return new[] { "La solicitud es obligatoria." };
        }

        return solicitud.Validar();
    }

    private ConfiguracionConexionRabbit ObtenerConfiguracionEfectiva(
        )
    {

            return _contextoConexion.ObtenerConfiguracionActual() ?? new ConfiguracionConexionRabbit();
    }

    private static string ResolverNombreCola(string? nombreSolicitado, ConfiguracionConexionRabbit configuracion)
    {
        var nombre = string.IsNullOrWhiteSpace(nombreSolicitado) ? configuracion.NombreColaPorDefecto : nombreSolicitado.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("No se proporcionó un nombre de cola válido.");
        }

        return nombre;
    }

    private IConnection CrearConexion(ConfiguracionConexionRabbit configuracion)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuracion.HostName,
            Port = configuracion.Puerto,
            UserName = configuracion.Usuario,
            Password = configuracion.Contrasena,
            VirtualHost = configuracion.VirtualHost,
            AutomaticRecoveryEnabled = true,
            DispatchConsumersAsync = true,
            ClientProvidedName = configuracion.NombreConexion,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, configuracion.TiempoMaximoEsperaSegundos))
        };

        if (configuracion.UsarSsl)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = configuracion.HostName
            };
        }

        return factory.CreateConnection(configuracion.NombreConexion);
    }

    private static void ConfigurarCalidadServicio(IModel canal, ConfiguracionConexionRabbit configuracion)
    {
        if (configuracion.Prefetch > 0)
        {
            canal.BasicQos(0, configuracion.Prefetch, global: false);
        }
    }

    private static QueueDeclareOk DeclararCola(IModel canal, string nombreCola, ConfiguracionConexionRabbit configuracion)
    {
        try
        {
            // Si la cola ya existe (quorum, classic, etc.), solo recuperamos su estado.
            return canal.QueueDeclarePassive(nombreCola);
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
        {
            // No existe: la creamos con la configuración actual.
            return canal.QueueDeclare(
                   queue: nombreCola,
                   durable: configuracion.PersistirMensajes,
                   exclusive: false,
                   autoDelete: false,
                   arguments: null);
           }
       }

    private static byte[] CodificarContenido(JsonElement contenido)
    {
        var texto = contenido.GetRawText();
        return Encoding.UTF8.GetBytes(texto);
    }

    private static string DecodificarMensaje(ReadOnlyMemory<byte> cuerpo)
    {
        return Encoding.UTF8.GetString(cuerpo.Span);
    }

    private IBasicProperties CrearPropiedadesMensaje(IModel canal, SolicitudOperacionCola solicitud, ConfiguracionConexionRabbit configuracion)
    {
        var propiedades = canal.CreateBasicProperties();
        propiedades.ContentType = "application/json";
        propiedades.DeliveryMode = (byte)(configuracion.PersistirMensajes ? 2 : 1);
        propiedades.MessageId = Guid.NewGuid().ToString("N");
        propiedades.AppId = configuracion.NombreConexion;
        propiedades.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        propiedades.Persistent = configuracion.PersistirMensajes;

        var metadatos = solicitud.Metadatos ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (metadatos.Count > 0)
        {
            propiedades.Headers = metadatos
                .Where(par => !string.IsNullOrWhiteSpace(par.Key))
                .ToDictionary(par => par.Key, par => (object?)(par.Value ?? string.Empty), StringComparer.OrdinalIgnoreCase);
        }

        return propiedades;
    }

    private async Task RegistrarErrorAsync(string clase, string metodo, Exception excepcion, CancellationToken cancellationToken)
    {
        _logger?.LogError(excepcion, "Error en {Clase}.{Metodo}", clase, metodo);
        await _administradorLog.RegistrarEventoAsync(NivelLog.Error, clase, metodo, excepcion.Message, excepcion, cancellationToken)
            .ConfigureAwait(false);
    }
}