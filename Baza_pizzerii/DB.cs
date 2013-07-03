using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace Baza_pizzerii
{
    static class DB
    {
        public static NpgsqlConnection loginUserToDB(string username, string password)
        {
            string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                    "localhost", "5432", username, password, "bazapizzerii");
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            return conn;
        }
        public static NpgsqlConnection loginAppUserToDB() {
            return DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"]);
        }
    }
}
