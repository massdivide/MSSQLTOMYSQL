

using Microsoft.Data.SqlClient;
using MySqlConnector;

public class MSSQLtoMySQLConverter
{
    public static void Convert(string mssqlConnectionString, string mysqlConnectionString)
    {
        // Connect to the MSSQL database
        using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
        {
            mssqlConnection.Open();

            // Connect to the MySQL database
            using (MySqlConnection mysqlConnection = new MySqlConnection(mysqlConnectionString))
            {
                mysqlConnection.Open();

                // Retrieve the list of tables from the MSSQL database
                using (SqlCommand mssqlCommand = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", mssqlConnection))
                {
                    using (SqlDataReader mssqlReader = mssqlCommand.ExecuteReader())
                    {
                        while (mssqlReader.Read())
                        {
                            string tableName = mssqlReader.GetString(0);

                            // Retrieve the data from the MSSQL table
                            using (SqlCommand mssqlDataCommand = new SqlCommand($"SELECT * FROM {tableName}", mssqlConnection))
                            {
                                using (SqlDataReader mssqlDataReader = mssqlDataCommand.ExecuteReader())
                                {
                                    // Create the MySQL table
                                    using (MySqlCommand mysqlCreateTableCommand = new MySqlCommand($"CREATE TABLE {tableName} (", mysqlConnection))
                                    {
                                        for (int i = 0; i < mssqlDataReader.FieldCount; i++)
                                        {
                                            string columnName = mssqlDataReader.GetName(i);
                                            string columnType = mssqlDataReader.GetDataTypeName(i);

                                            mysqlCreateTableCommand.CommandText += $"{columnName} {columnType}";

                                            if (i < mssqlDataReader.FieldCount - 1)
                                            {
                                                mysqlCreateTableCommand.CommandText += ", ";
                                            }
                                        }

                                        mysqlCreateTableCommand.CommandText += ")";

                                        mysqlCreateTableCommand.ExecuteNonQuery();
                                    }

                                    // Insert the data into the MySQL table
                                    while (mssqlDataReader.Read())
                                    {
                                        using (MySqlCommand mysqlInsertCommand = new MySqlCommand($"INSERT INTO {tableName} VALUES (", mysqlConnection))
                                        {
                                            for (int i = 0; i < mssqlDataReader.FieldCount; i++)
                                            {
                                                string columnValue = mssqlDataReader.GetValue(i).ToString();

                                                mysqlInsertCommand.CommandText += $"'{columnValue}'";

                                                if (i < mssqlDataReader.FieldCount - 1)
                                                {
                                                    mysqlInsertCommand.CommandText += ", ";
                                                }
                                            }

                                            mysqlInsertCommand.CommandText += ")";

                                            mysqlInsertCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        string mssqlConnectionString = "Data Source=server;Initial Catalog=database;User ID=username;Password=password";
        string mysqlConnectionString = "Server=server;Database=database;Uid=username;Pwd=password";

        MSSQLtoMySQLConverter.Convert(mssqlConnectionString, mysqlConnectionString);

        Console.WriteLine("Conversion completed successfully.");
    }
}
