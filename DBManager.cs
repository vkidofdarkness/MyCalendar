using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data;
using System.Windows;
using System.Data.SQLite;

namespace MyCalendar
{
    public class DBManager
    {
        public List<CalendarDay> GetDay(DateTime date)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("CalendarDB")))
            {
                return connection.Query<CalendarDay>($"SELECT * FROM CalendarDay WHERE Date = '{date}'").ToList();
            }
        }

        public void AddEvent(string Name, string Description, string _date)
        {
            DateTime Date = DateTime.Parse(_date);
            int CalendarDayId = FindDay(Date);
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("CalendarDB")))
            {
                connection.Execute("INSERT INTO Event (Name, Description, CalendarDayID) VALUES(@Name ,@Description, @CalendarDayId)", new { Name, Description, CalendarDayId});
            }
        }

        public List<Event> RetrieveEventDataByDate(DateTime _date)
        {
            List<Event> returnEvents = new List<Event>();
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("CalendarDB")))
            {
              
                int calendarDayId = FindDay(_date);

                string eventQuery = "SELECT * FROM Event WHERE CalendarDayId = @CalendarDayId";
                returnEvents = connection.Query<Event>(eventQuery, new { CalendarDayId = calendarDayId }).ToList();
            }
            return returnEvents;
        }

        public void SearchEvent(string SearchName, string date)
        {
            DateTime _date = DateTime.Parse(date);
            using IDbConnection connection = new SQLiteConnection(Helper.CnnVal("CalendarDB"));
            string query = "SELECT Event.*, CalendarDay.Id AS CalendarDayId, CalendarDay.Date AS CalendarDate " +
                           "FROM Event " +
                           "INNER JOIN CalendarDay ON Event.CalendarDayId = CalendarDay.Id " +
                           "WHERE Event.Name = @Name AND CalendarDay.Date = @Date";

            var result = connection.QueryFirstOrDefault(query, new { Name = SearchName, Date = _date });

            if (result != null)
            {
                int eventId = (int)result.Id;
                string eventDescription = result.Description;
                DateTime storedEventDate = result.CalendarDate;

                string returnData = SearchName + ": " + eventDescription + "\n Date:" + storedEventDate.ToString();
                MessageBox.Show(returnData);
            }
            else
            {
                Console.WriteLine("Соответствующая запись не найдена.");
                MessageBox.Show("Не удалось найти задачу.");

            }
        }

        public int FindDay(DateTime date)
        {
            int CalendarDayId = -1;
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("CalendarDB")))
            {
                try
                {
                    bool recordExists = connection.QueryFirstOrDefault<bool>($"SELECT CASE WHEN EXISTS (SELECT Id FROM CalendarDay WHERE Date = @Date) THEN 1 ELSE 0 END", new { Date = date });

                    if (recordExists)
                    {
                        CalendarDayId = connection.QueryFirstOrDefault<int>("SELECT Id FROM CalendarDay WHERE Date = @Date", new { Date = date });
                    }
                    else
                    {
                        connection.Execute("INSERT INTO CalendarDay (Date) VALUES (@Date)", new { Date = date });
                        CalendarDayId = connection.QueryFirstOrDefault<int>("SELECT Id FROM CalendarDay WHERE Date = @Date", new { Date = date });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            return CalendarDayId;
        }

        public void RemoveEventAndCalendarDay(string eventName, string _date)
        {
            DateTime eventDate = DateTime.Parse(_date);

            using (IDbConnection connection = new System.Data.SQLite.SQLiteConnection(Helper.CnnVal("CalendarDB")))
            {
                connection.Open();
                IDbTransaction transaction = connection.BeginTransaction();

                try
                {
                    string calendarDayQuery = "SELECT Id FROM CalendarDay WHERE Date = @EventDate";
                    int calendarDayId = connection.QueryFirstOrDefault<int>(calendarDayQuery, new { EventDate = eventDate }, transaction);

                    if (calendarDayId != 0)
                    {
                        string removeEventQuery = "DELETE FROM Event WHERE Name = @EventName AND CalendarDayId = @CalendarDayId";
                        connection.Execute(removeEventQuery, new { EventName = eventName, CalendarDayId = calendarDayId }, transaction);
                        MessageBox.Show("Успешно удалено.");

                        string remainingEventsQuery = "SELECT COUNT(*) FROM Event WHERE CalendarDayId = @CalendarDayId";
                        int remainingEvents = connection.ExecuteScalar<int>(remainingEventsQuery, new { CalendarDayId = calendarDayId }, transaction);

                        if (remainingEvents == 0)
                        {
                            string removeCalendarDayQuery = "DELETE FROM CalendarDay WHERE Id = @CalendarDayId";
                            connection.Execute(removeCalendarDayQuery, new { CalendarDayId = calendarDayId }, transaction);
                        }
                        transaction.Commit();
                    }
                    else
                    {
                        Console.WriteLine("В таблице не найдены соответствующие записи.");
                        transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    transaction.Rollback();
                }
            }
        }
    }
}



