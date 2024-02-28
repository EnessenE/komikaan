using komikaan.Context;
using komikaan.Handlers;
using komikaan.Interfaces;
using komikaan.Services;
using Refit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var corsName = "sources";

builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

Log.Logger.Information("Starting application");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsName,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200",
                    "https://volt.reasulus.nl")
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
        httpClient.BaseAddress = new Uri("https://gateway.apiportal.ns.nl");
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", builder.Configuration.GetValue<string>("ns_api_key"));
        // Will throw `TaskCanceledException` if the request goes longer than 3 seconds.
        httpClient.Timeout = TimeSpan.FromSeconds(10);
    });
// Adding our new handler here
refitClientBuilder.AddHttpMessageHandler(serviceProvider
    => new HttpLoggingHandler(serviceProvider.GetRequiredService<ILogger<HttpLoggingHandler>>()));
refitClientBuilder.Services.AddSingleton<HttpLoggingHandler>();
builder.Services.AddSingleton<IDataSupplierContext, NSContext>();
builder.Services.AddHostedService<DataService>();


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
