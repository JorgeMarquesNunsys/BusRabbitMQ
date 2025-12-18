using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BusRabbitMQ.Shared.Enumeraciones;

namespace BusRabbitMQ.Shared.Models;

public sealed class SolicitudOperacionCola
{
    private IDictionary<string, string?> _metadatos = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public string? NombreCola { get; init; }
    public JsonElement Contenido { get; init; }
    public bool PermitirPublicarSinSuscriptores { get; init; }

    public IDictionary<string, string?> Metadatos
    {
        get => _metadatos;
        init => _metadatos = value ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> Validar(TipoOperacionCola tipoOperacion)
    {
        var errores = new List<string>();

        if (tipoOperacion is TipoOperacionCola.Enviar or TipoOperacionCola.Publicar)
        {
            if (Contenido.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                errores.Add("El contenido del mensaje es obligatorio.");
            }
        }

        if (!string.IsNullOrWhiteSpace(NombreCola) && NombreCola!.Length > 255)
        {
            errores.Add("El nombre de la cola no puede exceder 255 caracteres.");
        }

        if (_metadatos.Any(par => string.IsNullOrWhiteSpace(par.Key)))
        {
            errores.Add("Los metadatos deben contener claves válidas.");
        }

        return errores;
    }
}