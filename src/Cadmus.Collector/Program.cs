using Cadmus.Collector.State;
using Cadmus.Collector;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Cadmus Collector";
});

var cadmusApiBaseUrl = builder.Configuration["CadmusApi:BaseUrl"]
    ?? throw new InvalidOperationException(
        "A configuração CadmusApi:BaseUrl é obrigatória.");

builder.Services.AddHttpClient("CadmusApi", client =>
{
    client.BaseAddress = new Uri(cadmusApiBaseUrl);
});

builder.Services.AddSingleton<CollectorCheckpointStore>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();