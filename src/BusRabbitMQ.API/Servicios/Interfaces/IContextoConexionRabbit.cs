using BusRabbitMQ.Shared.Models;

namespace BusRabbitMQ.API.Servicios.Interfaces;

public interface IContextoConexionRabbit
{
    ConfiguracionConexionRabbit ObtenerConfiguracionActual();

    Task<ResultadoOperacionCola<ConfiguracionConexionRabbit>> ActualizarConfiguracionAsync(
        ConfiguracionConexionRabbit nuevaConfiguracion,
        CancellationToken cancellationToken);

    Task<ResultadoOperacionCola<ConfiguracionConexionRabbit>> RestablecerConfiguracionPredeterminadaAsync(
        CancellationToken cancellationToken);
}