using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MutationTestingTDD.Data;

namespace MutationTestingTDD
{
    public static class DbContextConfigurator
    {
        public static void ConfigureDbContextUsingInMemorySQLite(this IServiceCollection services)
        {
            services.AddDbContext<WarehouseDbContext>(options =>
                {
                    var connectionString = $"datasource=inmemorydb{Guid.NewGuid()};mode=memory;";
                    var connection = new SqliteConnection(connectionString);
                    connection.Open();
                    options.UseSqlite(connection);
                    options.EnableSensitiveDataLogging();
                    //Console.WriteLine("INFO - Using inmemory SQLite");
                },
                ServiceLifetime.Scoped);
        }
    }
}
