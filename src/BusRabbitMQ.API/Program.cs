using BusRabbitMQ.API.Extensiones;

var builder = WebApplication.CreateBuilder(args);

builder.RegistrarServiciosAplicacion();

var app = builder.Build();

app.ConfigurarPipelineAplicacion();

app.Run();
