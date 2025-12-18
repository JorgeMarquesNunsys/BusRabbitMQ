namespace BusRabbitMQ.Shared.Models;

public sealed class EstadoColaDetalle
{
    public string NombreCola { get; init; } = string.Empty;
    public uint TotalMensajesListos { get; init; }
    public uint TotalMensajesEnProcesamiento { get; init; }
    public IReadOnlyCollection<string> ResumenMensajes { get; init; } = Array.Empty<string>();

    public bool TieneMensajes => TotalMensajesListos + TotalMensajesEnProcesamiento > 0;
}