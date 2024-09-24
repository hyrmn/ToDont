using Fluid.ViewEngine;
using Serilog;
using Serilog.Events;
using Serilog.Templates.Themes;
using SerilogTracing;
using SerilogTracing.Expressions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.WithProperty("Application", "ToDont")
    .WriteTo.Console(Formatters.CreateConsoleTextFormatter(theme: TemplateTheme.Code))
    .CreateLogger();

using var listener = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .TraceToSharedLogger();

Log.Information("Starting application");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();

    builder.Services.Configure<ConnectionStrings>(
        builder.Configuration.GetSection("ConnectionStrings")
    );

    builder.Services.AddHostedService<DbMigratorHost>();

    builder.Services.AddFluid(config =>
    {
        config.ViewsFileProvider = new FileProviderMapper(
            builder.Environment.ContentRootFileProvider,
            "Features/"
        );
        config.PartialsFileProvider = new FileProviderMapper(
            builder.Environment.ContentRootFileProvider,
            "Features/"
        );
    });

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.MapGet(
        "/",
        () => Results.Extensions.View("ToDoLists/ShowLists", new Todo(1, "Go back to work!", false))
    );

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

record Todo(int Id, string Name, bool IsComplete);

sealed class ConnectionStrings
{
    public required string ToDosDb { get; init; }
}
