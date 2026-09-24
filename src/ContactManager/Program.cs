using ContactManager.Infrastructure;
using ContactManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ContactManager;

internal static class Program
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    private static void Main()
    {
        var logger = LogManager.GetCurrentClassLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Alle EF-Core-Migrationen werden beim Start angewendet. Dadurch bleiben die Daten dauerhaft in SQLite gespeichert und das Schema ist auf dem erwarteten Stand.
            
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            Application.Run(ServiceProvider.GetRequiredService<FrmMain>());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Programm wegen eines unbehandelten Fehlers beendet.");
            MessageBox.Show($"Die Anwendung konnte nicht gestartet werden:\n{ex.Message}",
                "Startfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ServiceProvider?.Dispose();
            LogManager.Shutdown();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddTransient<FrmMain>();
        services.AddTransient<FrmSaveEmployee>();
        services.AddTransient<FrmSaveCustomer>();
        services.AddTransient<FrmCustomerNotes>();

        services.AddTransient<ApplicationDbContext>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<ICustomerService, CustomerService>();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            loggingBuilder.AddNLog();
        });
    }
}
