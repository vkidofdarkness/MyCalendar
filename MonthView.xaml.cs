using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace MyCalendar
{
    public partial class MonthView : Page
    {
        public int _month { get; set; }
        public int _year {get; set; }


        public MonthView()
        {
            InitializeComponent();

            _month = DateTime.Now.Month;
            _year = DateTime.Now.Year;

            MonthLabel.Content = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.ToString("yyyy");

            PopulateCalendarGrid(_year, _month);
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            int prevMonth, prevYear;

            if(_month != 1)
            {
                prevMonth = _month - 1;
                _month = prevMonth;
            }

            else 
            { 
                _month = 12;
                prevYear = _year - 1;
                _year = prevYear;
            }

            DateTime _Date = new DateTime(_year, _month, 1);
            MonthLabel.Content = _Date.ToString("MMMM") + " " + _Date.ToString("yyyy");
            PopulateCalendarGrid(_year, _month);
            
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            int nextMonth, nextYear;

            if (_month != 12)
            {
                nextMonth = _month + 1;
                _month = nextMonth;
            }

            else
            {
                _month = 1;
                nextYear = _year + 1;
                _year = nextYear;
            }

            DateTime _Date = new DateTime(_year, _month, 1);
            MonthLabel.Content = _Date.ToString("MMMM") + " " + _Date.ToString("yyyy");
            PopulateCalendarGrid(_year, _month);

        }

        private void OpenDropDown_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu dropdownMenu = new ContextMenu();

            Button button1 = new Button { Content = "Месяц", Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, Style = (Style)FindResource("HoverButtonDDStyle") };
            Button button2 = new Button { Content = "Неделя", Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, Style = (Style)FindResource("HoverButtonDDStyle") };
            Button button3 = new Button { Content = "День", Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, Style = (Style)FindResource("HoverButtonDDStyle") };

            button1.Click += DropdownButton_Click;
            button2.Click += DropdownButton_Click;
            button3.Click += DropdownButton_Click;

            dropdownMenu.Items.Add(button1);
            dropdownMenu.Items.Add(button2);
            dropdownMenu.Items.Add(button3);

            dropdownMenu.PlacementTarget = sender as UIElement;
            dropdownMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            dropdownMenu.IsOpen = true;
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            ((ContextMenu)((FrameworkElement)sender).Parent).IsOpen = false;

            if (clickedButton != null)
            {
                string buttonText = clickedButton.Content.ToString();

                if (buttonText == "Месяц")
                {
                    NavigationService.Navigate(new MonthView());
                }

                else if (buttonText == "Неделя")
                {
                    NavigationService.Navigate(new WeekView());
                }

                else if (buttonText == "День")
                {
                    NavigationService.Navigate(new DailyView());
                }
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            AddingTask addwindow = new AddingTask();
            addwindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = 10;
            this.Effect = blurEffect;

            addwindow.ShowDialog();

            this.Effect = null;

            PopulateCalendarGrid(_year, _month);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Search searchwindow = new Search();
            searchwindow.Show();
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Upload upload = new Upload();
            upload.WindowStartupLocation = WindowStartupLocation.CenterScreen;
 
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = 10;
            this.Effect = blurEffect;

            upload.ShowDialog();

            this.Effect = null;

            PopulateCalendarGrid(_year, _month);
        }

        public void PopulateCalendarGrid(int year, int month)
        {
            DBManager databaseManager = new DBManager();
            
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek - 1;

            int daysInPreviousMonth = startingDayOfWeek;
            DateTime firstDayOfPreviousMonth = firstDayOfMonth.AddDays(-daysInPreviousMonth);

            int totalCells = 6 * 7;

            CalendarGrid.Children.Clear();

            for (int day = 1; day <= totalCells; day++)
            {
                int row = (day - 1) / 7;
                int col = (day - 1) % 7;

                DateTime currentDate = firstDayOfPreviousMonth.AddDays(day - 1);

                var tasksForDate = databaseManager.RetrieveEventDataByDate(currentDate);
                string taskNames = string.Join("\n", tasksForDate.Select(t => t.name));

                Button dateLabel = new Button
                {
                    Content = currentDate.Day.ToString(),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.Gray,
                    Foreground = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };

                dateLabel.Content += "\n" + taskNames;

                if (currentDate.Month != month)
                {
                    dateLabel.Foreground = Brushes.LightGray; 
                    dateLabel.BorderBrush = Brushes.LightGray;
                }
                else if (currentDate == DateTime.Today)
                {
                    dateLabel.Foreground = Brushes.Black;
                    dateLabel.BorderBrush = Brushes.Black;
                    dateLabel.BorderThickness = new Thickness(2);
                }

                CalendarGrid.Children.Add(dateLabel);
                Grid.SetRow(dateLabel, row);
                Grid.SetColumn(dateLabel, col);
            }
        }

    }
}
