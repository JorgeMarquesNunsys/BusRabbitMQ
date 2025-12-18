using BusRabbitMQ.Shared.Models;
using RabbitMQ.Client;

namespace BusRabbitMQ.API.Servicios.Interfaces;

public interface IFabricaConexionRabbit
{
    IConnection CrearConexion(ConfiguracionConexionRabbit configuracion);
}