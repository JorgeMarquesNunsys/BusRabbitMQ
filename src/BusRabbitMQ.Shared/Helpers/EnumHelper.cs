namespace BusRabbitMQ.Shared.Helpers;

public static class EnumHelper
{
    public static bool TryObtenerEnum<TEnum>(string? valor, out TEnum resultado)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            resultado = default;
            return false;
        }

        return Enum.TryParse(valor, true, out resultado);
    }

    public static IReadOnlyCollection<string> ObtenerValoresDisponibles<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetNames(typeof(TEnum));
    }
}