using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Clinic.DB
{
    public static class DbHelper
    {
        /// <summary>
        /// Повертає кількість рядків у вказаній таблиці.
        /// </summary>
        public static int CountRows(MySqlConnection conn, string tableName)
        {
            using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {tableName}", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Перевіряє, чи існує запис за умовою WHERE.
        /// </summary>
        public static bool Exists(MySqlConnection conn, string tableName, string whereClause, Dictionary<string, object> parameters)
        {
            string query = $"SELECT 1 FROM {tableName} WHERE {whereClause} LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);

            foreach (var p in parameters)
                cmd.Parameters.AddWithValue(p.Key, p.Value);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }

        /// <summary>
        /// Виконує запит INSERT/UPDATE/DELETE.
        /// </summary>
        public static int ExecuteNonQuery(MySqlConnection conn, string sql, Dictionary<string, object> parameters)
        {
            using var cmd = new MySqlCommand(sql, conn);

            foreach (var p in parameters)
                cmd.Parameters.AddWithValue(p.Key, p.Value);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Повертає список значень з одного стовпця (наприклад, імена).
        /// </summary>
        public static List<string> GetColumnValues(MySqlConnection conn, string sql)
        {
            var result = new List<string>();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                result.Add(reader.GetString(0));

            return result;
        }
    }
}
