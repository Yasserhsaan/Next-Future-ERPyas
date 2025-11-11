using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Factories
{
    public static class DbContextFactory
    {
        public static AppDbContext Create()
        {
            var settings = SettingsService.Load();
            var connectionString = BuildConnectionString(settings);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        public static bool TryConnect(out string? errorMessage)
        {
            try
            {

                using var context = Create();
                context.Database.OpenConnection();
                context.Database.CloseConnection();

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string BuildConnectionString(ConnectionSettings settings)
        {
            var server = settings.Server;
            if (string.IsNullOrWhiteSpace(server))
            {
                // Default to local SQL Server
                server = @"localhost"; // or .\SQLEXPRESS if using Express named instance
            }

            if (settings.Port.HasValue)
            {
                server = $"{server},{settings.Port.Value}";
            }

            var database = string.IsNullOrWhiteSpace(settings.Database) ? "NextFutureERP" : settings.Database;

            // Normalize type value; accept "Local", "Server", "Locals"
            var type = (settings.Type ?? "Server").Trim();
            var isIntegrated = string.Equals(type, "Local", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(type, "Locals", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(type, "Windows", StringComparison.OrdinalIgnoreCase);

            var common = ";TrustServerCertificate=True;MultipleActiveResultSets=True";

            if (isIntegrated)
            {
                return $"Server={server};Database={database};Integrated Security=True{common}";
            }

            var user = settings.Username ?? string.Empty;
            var pass = settings.Password ?? string.Empty;
            return $"Server={server};Database={database};User Id={user};Password={pass}{common}";
        }
    }
}
