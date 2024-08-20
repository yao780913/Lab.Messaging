using Microsoft.Extensions.Hosting;

var hostBuilder = new HostBuilder();

var host = hostBuilder
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
    })
    .Build();

var json = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");


host.Run();