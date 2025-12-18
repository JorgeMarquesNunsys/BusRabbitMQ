using System.Threading;
using System.Threading.Tasks;
using BusRabbitMQ.Shared.Models;

namespace BusRabbitMQ.API.Servicios.Interfaces;

public interface IServicioColaRabbit
{
    Task<ResultadoOperacionCola<string>> EnviarAsync(SolicitudOperacionCola solicitud, CancellationToken cancellationToken);
    Task<ResultadoOperacionCola<bool>> PublicarAsync(SolicitudOperacionCola solicitud, CancellationToken cancellationToken);
    Task<ResultadoOperacionCola<EstadoColaDetalle>> SuscribirAsync(SolicitudEstadoCola solicitud, CancellationToken cancellationToken);
}