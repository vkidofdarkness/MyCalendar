using System.Windows;
using System.Windows.Controls;

namespace MyCalendar
{
    public partial class AddingTask : Window
    {
        private string _title = "";
        private string _description = "";
        private DBManager _databaseManager = new DBManager();
        public AddingTask()
        {
            InitializeComponent();
        }

        public void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            string _TaskName = TaskName.Text;
            string _TaskDescription = TaskDescription.Text;
            string _TaskDate = TaskDate.Text;

            if (_TaskName != "" && _TaskDate != "")
            {
                _databaseManager.AddEvent(_TaskName, _TaskDescription, _TaskDate);
                MessageBox.Show("Задача добавлена.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите название и дату.");
            }
        }

        private void TaskName_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void CancelTask_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DateInput_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}