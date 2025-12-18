using BusRabbitMQ.API.Extensiones;

var builder = WebApplication.CreateBuilder(args);

builder.RegistrarServiciosAplicacion();

var app = builder.Build();

app.ConfigurarPipelineAplicacion();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
