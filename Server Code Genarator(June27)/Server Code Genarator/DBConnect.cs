using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace Server_Code_Genarator
{
    public class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        public string uid;
        public string password;


        public DBConnect(String user, String pass)
        {
            uid = user;
            password = pass;
            Initialize();
        }

        public void SetDatabase(String DbName)
        {
            database = DbName;
        }

        private void Initialize()
        {
            server = "localhost";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        public string GetUsername()
        {
            return uid;
        }

        public string GetPassword()
        {
            return password;
        }

        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        public String GetAllSchema()
        {
            String schema = "";
            if(OpenConnection())
                using (MySqlCommand cmd = new MySqlCommand("SELECT SCHEMA_NAME AS `Database` FROM INFORMATION_SCHEMA.SCHEMATA;", connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder sb = new StringBuilder();
                        while (reader.Read())
                        {
                           schema += reader[0].ToString() + "|";
                        }
                    }
                }
            CloseConnection();
            return schema;
        }

        public String GetAllTables()
        {
            String schema = "";
            if (OpenConnection())
                using (MySqlCommand cmd = new MySqlCommand("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = @dbname;", connection))
                {
                    cmd.Parameters.AddWithValue("@dbname",database);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder sb = new StringBuilder();
                        while (reader.Read())
                        {
                            schema += reader[0].ToString() + "|";
                        }
                    }
                }
            CloseConnection();
            return schema;
        }

        public String GetAllAttributes(string table_)
        {
            string attrs = "";
            if (OpenConnection())
                using (MySqlCommand cmd = new MySqlCommand("SELECT column_name, data_type, character_maximum_length, is_nullable, column_key FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = @tablename;", connection))
                {
                    cmd.Parameters.AddWithValue("@tablename",table_);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder sb = new StringBuilder();
                        while (reader.Read())
                        {
                            string dfname = reader[0].ToString();
                            string dftype = reader[1].ToString();
                            string dfsize = reader[2].ToString();
                            string dfmand = reader[3].ToString();
                            string dfprim = reader[4].ToString();
                            attrs += dfname + "^" + dftype + "^"+ dfsize + "^" + dfmand + "^" + dfprim + "|";
                        }
                    }
                }
            CloseConnection();
            return attrs;
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
