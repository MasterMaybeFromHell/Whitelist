using System.Text.Json;

// DO NOT CHANGE

namespace MasterHell.Config
{
    public class ConfigManager
    {
        public virtual Config Load(string configPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);

                    return JsonSerializer.Deserialize<Config?>(json) ?? new Config();
                }
                catch { }
            }

            return Save(new Config(), configPath);
        }

        public virtual Config Save(Config config, string configPath)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);

            return config;
        }
    }
}