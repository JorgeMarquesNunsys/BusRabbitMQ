using System.Collections.Generic;
using System.Linq;

namespace BusRabbitMQ.Shared.Models;

public sealed class ResultadoOperacionCola<TValor>
{
    private ResultadoOperacionCola(bool esExitoso, string mensaje, TValor? valor, IReadOnlyCollection<string> errores)
    {
        EsExitoso = esExitoso;
        Mensaje = mensaje;
        Valor = valor;
        Errores = errores;
    }

    public bool EsExitoso { get; }
    public string Mensaje { get; }
    public TValor? Valor { get; }
    public IReadOnlyCollection<string> Errores { get; }

    public static ResultadoOperacionCola<TValor> CrearExito(TValor valor, string mensaje = "Operación completada.")
    {
        return new ResultadoOperacionCola<TValor>(true, mensaje, valor, Array.Empty<string>());
    }

    public static ResultadoOperacionCola<TValor> CrearFallo(IEnumerable<string>? errores, string mensaje = "Ocurrió un error al procesar la operación.")
    {
        var listaErrores = errores?
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .ToArray() ?? Array.Empty<string>();

        return new ResultadoOperacionCola<TValor>(false, mensaje, default, listaErrores);
    }
}