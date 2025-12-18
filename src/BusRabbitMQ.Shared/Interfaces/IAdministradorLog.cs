using BusRabbitMQ.Shared.Enumeraciones;

namespace BusRabbitMQ.Shared.Interfaces;

public interface IAdministradorLog
{
    Task RegistrarEventoAsync(
        NivelLog tipoError,
        string? nombreClase,
        string? nombreMetodo,
        string mensaje,
        Exception? excepcion = null,
        CancellationToken cancellationToken = default);
}