using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models
{
    internal class DatabaseConnection
    {
        private static readonly string ConnectionString =
            "Server=localhost;Port=3306;Database=agile_db;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
