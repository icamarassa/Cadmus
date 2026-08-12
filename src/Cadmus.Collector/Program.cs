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

var cadmusApiKey = builder.Configuration["CadmusApi:ApiKey"]
    ?? throw new InvalidOperationException(
        "A configuração CadmusApi:ApiKey é obrigatória.");

builder.Services.AddHttpClient("CadmusApi", client =>
{
    client.BaseAddress = new Uri(cadmusApiBaseUrl);

    client.DefaultRequestHeaders.Add(
        "X-Cadmus-Collector-Key",
        cadmusApiKey);
});