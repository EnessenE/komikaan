using komikaan.Context;
using komikaan.Handlers;
using komikaan.Interfaces;
using komikaan.Services;
using Refit;
using Serilog;
using System.Reflection;
using komikaan.Data.Configuration;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "I dont like top levels, will receive a reformat ever.")]
internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var corsName = "sources";

        builder.Configuration.AddEnvironmentVariables();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();

        Log.Logger.Information("Starting {app} {version} - {env}",
            Assembly.GetExecutingAssembly().GetName().Name,
            Assembly.GetExecutingAssembly().GetName().Version,
            builder.Environment.EnvironmentName);


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.Configure<SupplierConfigurations>(builder.Configuration.GetSection("SupplierMappings"));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(corsName,
                policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://komikaan.reasulus.nl",
                            "https://komikaan.nl")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        var refitSettings = new RefitSettings()
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer()
        };

        IHttpClientBuilder refitClientBuilder = builder.Services.AddRefitClient<INSApi>(refitSettings)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration.GetValue<string>("NSApiUrl")!);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", builder.Configuration.GetValue<string>("ns_api_key"));
                // Will throw `TaskCanceledException` if the request goes longer than 3 seconds.
                httpClient.Timeout = TimeSpan.FromSeconds(10);
            });
        // Adding our new handler here
        refitClientBuilder.AddHttpMessageHandler(serviceProvider
            => new HttpLoggingHandler(serviceProvider.GetRequiredService<ILogger<HttpLoggingHandler>>()));
        refitClientBuilder.Services.AddSingleton<HttpLoggingHandler>();
        AddDataSuppliers(builder);
        builder.Services.AddHostedService<DataService>();
        builder.Services.AddSingleton<IStopManagerService, StopManagerService>();
        builder.Services.AddSingleton<ITravelAdviceHandler, TravelAdviceHandler>();

        SetupApplication(builder, corsName);
    }

    private static void AddDataSuppliers(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDataSupplierContext, NSContext>();
    }

    private static void SetupApplication(WebApplicationBuilder builder, string corsName)
    {
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(corsName);

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseSerilogRequestLogging();
        app.MapControllers();

        app.Run();
    }
}
