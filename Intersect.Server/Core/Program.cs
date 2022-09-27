using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Intersect.Config;
using Intersect.Server.Database;
using Intersect.Server.Database.GameData;
using Intersect.Server.Database.Logging;
using Intersect.Server.Database.PlayerData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;

namespace Intersect.Server.Core
{
    /// <summary>
    /// Please do not modify this without JC's approval! If namespaces are referenced that are not SYSTEM.* then the server won't run cross platform.
    /// If you want to add startup instructions see Classes/ServerStart.cs
    /// </summary>
    public static partial class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Debugger.Launch();
            CultureInfo.DefaultThreadCurrentCulture = new("en-US");
            try
            {
                Bootstrapper.Start(args);
            }
            catch (Exception exception)
            {
                ServerContext.DispatchUnhandledException(exception, true);
            }
        }

        /// <summary>
        /// Host builder method for Entity Framework Design Tools to use when generating migrations.
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var rawDatabaseType = hostContext.Configuration.GetValue<string>("DatabaseType") ??
                                      DatabaseType.Sqlite.ToString();
                if (!Enum.TryParse(rawDatabaseType, out DatabaseType databaseType))
                {
                    throw new InvalidOperationException($"Invalid database type: {rawDatabaseType}");
                }

                var connectionString = hostContext.Configuration.GetValue<string>("ConnectionString");
                DbConnectionStringBuilder connectionStringBuilder = databaseType switch
                {
                    DatabaseType.MySql => new MySqlConnectionStringBuilder(connectionString),
                    DatabaseType.Sqlite => new SqliteConnectionStringBuilder(connectionString),
                    _ => throw new IndexOutOfRangeException($"Unsupported database type: {databaseType}")
                };

                DatabaseContextOptions databaseContextOptions = new()
                {
                    ConnectionStringBuilder = connectionStringBuilder, DatabaseType = databaseType
                };

                services.AddSingleton(databaseContextOptions);
            });
    }
}
