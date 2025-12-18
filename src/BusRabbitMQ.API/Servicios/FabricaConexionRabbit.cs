using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Models;
using RabbitMQ.Client;

namespace BusRabbitMQ.API.Servicios;

public sealed class FabricaConexionRabbit : IFabricaConexionRabbit
{
    public IConnection CrearConexion(ConfiguracionConexionRabbit configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

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
}