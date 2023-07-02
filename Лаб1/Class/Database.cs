using System.Data.SqlClient;

namespace Лаб1
{
    class Database
    {
        
        SqlConnection connection = new SqlConnection(@"Data Source=DESKTOP-DE6A90I\SQLEXPRESS;Initial Catalog=customersDB;Integrated Security=True");

        public void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }

        public void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        public SqlConnection GetConnection() { return connection; }
    }
}
