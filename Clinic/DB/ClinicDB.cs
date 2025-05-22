using MySql.Data.MySqlClient;

namespace Clinic.DB
{
    public static class ClinicDB
    {
        private const string ConnectionString =
            "server=localhost;" +
            "user=root;" +
            "password=Oleh_Shulha;" +
            "database=Clinic;" +
            "SslMode=none";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

    }
}
