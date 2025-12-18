namespace BusRabbitMQ.Shared.Models;

public sealed record MensajeGenerico<TContenido>
{
    public Guid IdMensaje { get; init; } = Guid.NewGuid();
    public DateTime FechaCreacionUtc { get; init; } = DateTime.UtcNow;
    public string? Origen { get; init; }
    public required TContenido Contenido { get; init; }
    public IReadOnlyDictionary<string, string?> Metadatos { get; init; } = new Dictionary<string, string?>();

    public static MensajeGenerico<TContenido> Crear(
        TContenido contenido,
        string? origen = null,
        IReadOnlyDictionary<string, string?>? metadatos = null)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        return new MensajeGenerico<TContenido>
        {
            Contenido = contenido,
            Origen = origen,
            Metadatos = metadatos ?? new Dictionary<string, string?>()
        };
    }
}