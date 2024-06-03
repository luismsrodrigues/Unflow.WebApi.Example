using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using NLog;
using Unflow.WebApi.Example;
using Unflow.WebApi.Example.Controllers;

using var loggerFactory = BuildNLogLoggerFactory();

var logger = loggerFactory.GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureServices(
        builder.Services,
        builder.Configuration,
        builder.Environment
    );

    await using var app = builder.Build();

    Configure(
        app,
        app.Environment
    );

    await app.RunAsync();
}
catch (Exception e)
{
    logger.Fatal(e, "Application failed to start");
    throw;
}

return;

static void ConfigureServices(
    IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment
)
{
    services.AddBusiness();
    
    services.AddUnflowWebApiDatabase(
        configuration.GetConnectionString("UnflowDotnetWebApi")
    );
    
    var swaggerVersion = $"v{Properties.FileVersionInfo.FileMajorPart}";
    services.AddSwaggerGen(o =>
    {
        o.SwaggerDoc(swaggerVersion, new OpenApiInfo
        {
            Title = "UnflowDotnetWebApi API",
            Version = swaggerVersion
        });
        var directory = Directory.GetParent(typeof(Program).Assembly.Location);
        if (directory == null)
            return;

        foreach (var xmlDocFile in directory.GetFiles()
                     .Where(fi =>
                         fi.Name.EndsWith(".Api.Contracts.xml", StringComparison.OrdinalIgnoreCase) ||
                         fi.Name.EndsWith(".Api.Controllers.xml", StringComparison.OrdinalIgnoreCase)
                     ))
        {
            o.IncludeXmlComments(xmlDocFile.FullName);
        }
    });

    services
        .AddMvcCore()
        .AddApiExplorer()
        .AddApplicationPart(typeof(ApiConstants).Assembly)
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
}

static LogFactory BuildNLogLoggerFactory()
{
    GlobalDiagnosticsContext.Set(
        "majorVersion",
        Properties.FileVersionInfo.ProductMajorPart.ToString(CultureInfo.InvariantCulture)
    );
    GlobalDiagnosticsContext.Set(
        "fileVersion",
        Properties.FileVersionInfo.FileVersion
    );
    GlobalDiagnosticsContext.Set(
        "productVersion",
        Properties.FileVersionInfo.ProductVersion
    );

    return LogManager.Setup(builder =>
    {
        builder.LoadConfigurationFromFile(optional: false);
    });
}

static void Configure(
    IApplicationBuilder app,
    IHostEnvironment env
)
{
    app.UseStaticFiles();

    app.UseRouting();

    app.UseSwagger(o => o.RouteTemplate = "/_swagger/{documentName}/swagger.json");

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"v{Properties.FileVersionInfo.FileMajorPart}/swagger.json", "UnflowDotnetWebApi API");
        c.RoutePrefix = "_swagger";
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}