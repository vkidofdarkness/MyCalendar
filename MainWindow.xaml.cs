using System;
using System.Windows;

namespace MyCalendar
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Проверяем, существует ли база данных в текущей папке, и если нет - создаем
            string dbFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CalendarDB.sqlite");
            Helper sqliteHelper = new Helper(dbFilePath);

            InitializeComponent();
            mainFrame.Navigate(new MonthView());
        }
    }
}





