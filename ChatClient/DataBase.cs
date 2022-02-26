using System;
using System.Data.SqlClient;
using System.Windows;

namespace ChatClient
{
    class DataBase
    {
        SqlConnection sqlConnection = new SqlConnection(@"Server=localhost;Database=Test;Trusted_Connection=Yes;");

        public void OpenConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    sqlConnection.Open();
                }
                catch
                {
                    MessageBox.Show("Error connecting to the database.\nCheck that the connection string is correct.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }
        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }
    }
}
