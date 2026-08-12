using Cadmus.Collector;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Cadmus Collector";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();