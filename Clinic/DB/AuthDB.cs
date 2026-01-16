using MySql.Data.MySqlClient;
using System;

namespace Clinic.DB
{
    public static class AuthDB
    {
        private static string GetConnectionString()
        {
            var server = Environment.GetEnvironmentVariable("MYSQL_HOST");
            
            var port = Environment.GetEnvironmentVariable("MYSQL_PORT");
            
            var password = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
            
            return $"server={server};" +
                   $"port={port};" +
                   $"user=root;" +
                   $"password={password};" +
                   $"database=ClinicAuth;" +
                   $"SslMode=none;" +
                   $"AllowPublicKeyRetrieval=true;" +
                   $"CharSet=utf8mb4;";
        }

        public static MySqlConnection GetConnection()
        {
            try
            {
                var connection = new MySqlConnection(GetConnectionString());
                connection.Open();
                connection.Close();
                return new MySqlConnection(GetConnectionString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to connect to the ClinicAuth database. Please verify that the Docker container is running. Error: {ex.Message}", ex);
            }
        }
    }
}
