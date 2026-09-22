using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var databaseType = AppDatabaseType.Sqlite;
        string connectionString;

       if (databaseType == AppDatabaseType.Sqlite)
		{
			var dbDir = DeviceInfo.Platform == DevicePlatform.WinUI
				? @"C:\DEV\AcademiaDoZe"
				: FileSystem.AppDataDirectory;

			if (!Directory.Exists(dbDir))
			{
				Directory.CreateDirectory(dbDir);
			}

			var dbPath = Path.Combine(dbDir, "db_academia_do_ze.db");
			connectionString = $"Data Source={dbPath};Mode=ReadWriteCreate;Cache=Shared;";
		}
        else
        {
            const string dbServer = "10.30.21.16";
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "root";
            const string dbPassword = "abcBolinhas12345";
            string dbComplemento = databaseType == AppDatabaseType.SqlServer
                ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"
                : "ConnectionTimeout=5;DefaultCommandTimeout=30;";

            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        });

        services.AddApplicationServices();
    }
}