using MySql.Data.MySqlClient;

namespace Clinic.DB
{
    public static class AuthDB
    {
        private const string ConnectionString =
            "server=localhost;" +
            "user=root;" +
            "password=Oleh_Shulha;" +
            "database=ClinicAuth;" +
            "SslMode=none";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

    }
}
