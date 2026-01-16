using System;
using System.IO;

namespace Clinic.DB
{
    public static class EnvLoader
    {
        public static void Load(string filePath = ".env")
        {
            string projectRoot = FindProjectRoot();
            string envPath = Path.Combine(projectRoot, filePath);

            if (!File.Exists(envPath))
            {
                throw new FileNotFoundException($".env file not found at: {envPath}");
            }

            foreach (var line in File.ReadAllLines(envPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }

        private static string FindProjectRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            while (!string.IsNullOrEmpty(currentDir))
            {
                string parentDir = Directory.GetParent(currentDir)?.FullName;
                if (parentDir == null)
                    break;

                if (File.Exists(Path.Combine(parentDir, ".env")) ||
                    File.Exists(Path.Combine(parentDir, "docker-compose.yml")))
                {
                    return parentDir;
                }

                currentDir = parentDir;
            }

            throw new DirectoryNotFoundException("Could not find project root directory containing .env or docker-compose.yml");
        }
    }
}
