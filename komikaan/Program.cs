﻿using komikaan.Context;
using komikaan.Handlers;
using komikaan.Interfaces;
using komikaan.Services;
using Serilog;
using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Text.Json.Serialization;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "I dont like top levels, will receive a reformat ever.")]
internal class Program
{
    public static void Main(string[] args)
    {
        var meter = new Meter("komikaan.api", "1.0.0");

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


        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.DefaultIgnoreCondition
                      = JsonIgnoreCondition.WhenWritingNull;
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        CreateObservability(builder, builder.Services, meter);

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

        AddDataSuppliers(builder);
        builder.Services.AddHostedService<DataService>();

        SetupApplication(builder, corsName);
    }

    private static void CreateObservability(IHostApplicationBuilder builder, IServiceCollection services,
        Meter meter)
    {
        var tracingOtlpEndpoint = builder.Configuration.GetValue<string>("OTLP_ENDPOINT_URL");

        var greeterActivitySource = new ActivitySource(meter.Name);

        var otel = services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource
            .AddService(builder.Environment.ApplicationName));

        // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
        otel.WithMetrics(metrics => metrics
            // Metrics provider from OpenTelemetry
            .AddAspNetCoreInstrumentation()
            .AddMeter(meter.Name)
            // Metrics provides by ASP.NET Core in .NET 8
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddPrometheusExporter());

        // Add Tracing for ASP.NET Core and our custom ActivitySource and export to Jaeger
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource(greeterActivitySource.Name);
            if (tracingOtlpEndpoint != null)
            {
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }
            else
            {
                tracing.AddConsoleExporter();
            }
        });
    }

    private static void AddDataSuppliers(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IGTFSContext, GTFSContext>();
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
        app.MapPrometheusScrapingEndpoint();

        app.UseCors(corsName);

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseSerilogRequestLogging();
        app.MapControllers();

        app.Run();
    }
}
