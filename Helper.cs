using System.Configuration;
using System.Data.SQLite;
using System.IO;


namespace MyCalendar
{
    public class Helper
    {

        private string connectionString;
        public static string? CnnVal(string name) 
        {
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        }

        public Helper(string dbFilePath)
        {
                
            if (File.Exists(dbFilePath))
            {
                // Файл базы данных уже существует
                connectionString = $"Data Source={dbFilePath};Version=3;";
            }
            else
            {
                // Файл базы данных не существует, создаем его
                CreateDatabase(dbFilePath);
            }
        }

        private void CreateDatabase(string dbFilePath)
        {
            SQLiteConnection.CreateFile(dbFilePath);

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
            {
                connection.Open();

                string createTableQuery = "CREATE TABLE IF NOT EXISTS CalendarDay (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Date DATETIME NOT NULL)";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Event (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Description TEXT NULL, CalendarDayId INTEGER NOT NULL, CONSTRAINT FK_Event_Day FOREIGN KEY (CalendarDayId) REFERENCES CalendarDay(Id))";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }


                connection.Close();
            }

            connectionString = $"Data Source={dbFilePath};Version=3;";
        }
    }
}
