namespace BusRabbitMQ.Shared.Models;

public sealed class ConfiguracionRegistroLog
{
    public string? DirectorioBase { get; init; }
    public bool IncluirStackTrace { get; init; } = true;

    public IReadOnlyCollection<string> Validar()
    {
        var errores = new List<string>();

        if (!string.IsNullOrWhiteSpace(DirectorioBase))
        {
            try
            {
                _ = Path.GetFullPath(DirectorioBase);
            }
            catch (Exception)
            {
                errores.Add("El directorio base para logs no es una ruta válida.");
            }
        }

        return errores;
    }

    public string ResolverDirectorio()
    {
        if (!string.IsNullOrWhiteSpace(DirectorioBase))
        {
            return DirectorioBase!;
        }

        return Path.Combine(AppContext.BaseDirectory, "logs");
    }
}