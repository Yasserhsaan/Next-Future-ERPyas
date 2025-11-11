using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Next_Future_ERP.Data.Models;

namespace Next_Future_ERP.Data.Services
{
    public class SettingsService
    {
        private static readonly string path = "appsettings.json";

        public static void Save(ConnectionSettings settings)
        {
            var json = JsonSerializer.Serialize(new { ConnectionSettings = settings }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static ConnectionSettings Load()
        {
            if (!File.Exists(path))
            {
               
                return new ConnectionSettings();
            }

            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("ConnectionSettings", out var settingsProp))
            {
                var deserializedSettings = JsonSerializer.Deserialize<ConnectionSettings>(settingsProp.GetRawText());
                return deserializedSettings ?? new ConnectionSettings(); // Ensure non-null return
            }

            return new ConnectionSettings();
        }
    }
}
