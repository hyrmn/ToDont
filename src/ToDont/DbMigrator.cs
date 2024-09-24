using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Support;
using Microsoft.Extensions.Options;

internal sealed class DbMigratorHost(
    IOptions<ConnectionStrings> connectionStrings,
    ILogger<DbMigratorHost> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Task.Run(
            () =>
            {
                var upgrader = DeployChanges
                    .To.SQLiteDatabase(connectionStrings.Value.ToDosDb)
                    .WithScriptsEmbeddedInAssembly(
                        Assembly.GetExecutingAssembly(),
                        script => script.StartsWith("ToDont.Migrations."),
                        new SqlScriptOptions { ScriptType = ScriptType.RunOnce }
                    )
                    .WithScriptsEmbeddedInAssembly(
                        Assembly.GetExecutingAssembly(),
                        script => script.StartsWith("ToDont.SqlScripts."),
                        new SqlScriptOptions { ScriptType = ScriptType.RunAlways }
                    )
                    .LogTo(new SerilogUpgradeLog(logger))
                    .Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    throw new Exception("Database upgrade failed", result.Error);
                }
            },
            stoppingToken
        );
}

internal sealed class SerilogUpgradeLog(ILogger logger) : IUpgradeLog
{
    public void WriteError(string message, params object[] args) => logger.LogError(message, args);

    public void WriteInformation(string message, params object[] args) =>
        logger.LogInformation(message, args);

    public void WriteWarning(string message, params object[] args) =>
        logger.LogWarning(message, args);
}
