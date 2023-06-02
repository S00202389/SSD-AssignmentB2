using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public class SaveLogs
    {
        private readonly static String dbName = "Banking Database.db";
        public string massageLog { get; set; }
        public string logTime { get; set; }

        public SaveLogs()
        {
           
        }

        public void saveLog(string massageLog, string logTime )
        {
            initialiseDatabase();

            using (var connection = getDatabaseConnection())
            {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    INSERT INTO saveLog VALUES(" +
                        "'" + massageLog + "', " +
                        "'" + logTime + "'" 
                       +")";


                    command.ExecuteNonQuery();
            }

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                  @"
                    CREATE TABLE IF NOT EXISTS Logs(    
                      message TEXT NOT NULL,
                      info TEXT NOT NULL
                    )
                  ";


                command.ExecuteNonQuery();

            }

        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = SaveLogs.dbName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

    }

}
