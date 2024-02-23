using System.Windows;
using System.Windows.Controls;

namespace MyCalendar
{
    public partial class Search : Window
    {
        private DBManager _databaseManager;
        public Search()
        {
            InitializeComponent();
            _databaseManager = new DBManager();
        }

        private void SearchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string _SearchName = SearchName.Text;
        }

        private void SearchDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string _SearchDate = SearchDate.Text;
        }

        private void CancelTask_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchTask_Click(object sender, RoutedEventArgs e)
        {
            string _SearchName = SearchName.Text;
            string _SearchDate = SearchDate.Text;
            if (_SearchName != "" && _SearchDate != "")
            {
                _databaseManager.SearchEvent(_SearchName, _SearchDate);
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите название и дату.");
            }
        }

        private void ScheduleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            string _SearchName = SearchName.Text;
            string _SearchDate = SearchDate.Text;
            if (_SearchName != "" && _SearchDate != "")
            {
                _databaseManager.RemoveEventAndCalendarDay(_SearchName, _SearchDate);
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите название и дату.");
            }
        }
    }
}