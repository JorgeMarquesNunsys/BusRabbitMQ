using System.Collections.Generic;

namespace BusRabbitMQ.Shared.Models;

public sealed class SolicitudEstadoCola
{
    public string? NombreCola { get; init; }
    public bool IncluirContenido { get; init; } = true;
    public int MaximoMensajes { get; init; } = 50;

    public IReadOnlyCollection<string> Validar()
    {
        var errores = new List<string>();

        if (!string.IsNullOrWhiteSpace(NombreCola) && NombreCola!.Length > 255)
        {
            errores.Add("El nombre de la cola no puede exceder 255 caracteres.");
        }

        if (MaximoMensajes <= 0)
        {
            errores.Add("El máximo de mensajes solicitados debe ser mayor a cero.");
        }
        else if (MaximoMensajes > 500)
        {
            errores.Add("El máximo de mensajes permitidos por consulta es 500.");
        }

        return errores;
    }
}