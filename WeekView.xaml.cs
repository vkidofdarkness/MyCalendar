using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace MyCalendar
{
    public partial class WeekView : Page
    {
        public string WeekRange { get; set; }
        private int _month { get; set; }
        private int _year { get; set; }
        private int _day { get; set; }
        public WeekView()
        {
            InitializeComponent();

            DateTime today = DateTime.Today;
            _month = today.Month;
            _year = today.Year;
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            DateTime endOfWeek = startOfWeek.AddDays(6);
            _day = startOfWeek.Day;

            WeekRange = $"{startOfWeek.ToShortDateString()} - {endOfWeek.ToShortDateString()}";
            WeekLabel.Content = WeekRange;

            PopulateCalendarGrid(today);
        }

        private void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            DateTime prevWeek = new DateTime(_year, _month, _day);
      
            prevWeek = prevWeek.AddDays(-7);
            _month = prevWeek.Month;
            _day = prevWeek.Day;
            _year = prevWeek.Year;

            PopulateCalendarGrid(prevWeek);
            DateTime prevWeekEnd = prevWeek.AddDays(6);
            WeekLabel.Content = $"{prevWeek.ToShortDateString()} - {prevWeekEnd.ToShortDateString()}";
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            DateTime nextWeek = new DateTime(_year, _month, _day);

            nextWeek = nextWeek.AddDays(7);
            _month = nextWeek.Month;
            _day = nextWeek.Day;
            _year = nextWeek.Year;

            PopulateCalendarGrid(nextWeek);
            DateTime nextWeekEnd = new DateTime();
            nextWeekEnd = nextWeek.AddDays(6);
            WeekLabel.Content = $"{nextWeek.ToShortDateString()} - {nextWeekEnd.ToShortDateString()}";
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
            Button? clickedButton = sender as Button;

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

            PopulateCalendarGrid(new DateTime(_year, _month, _day));
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

            PopulateCalendarGrid(new DateTime(_year, _month, _day));
        }

        private void PopulateCalendarGrid(DateTime dayX)
        { 
            DateTime firstDayOfWeek = dayX.AddDays(-(int)dayX.DayOfWeek + 1);

            CalendarGrid.Children.Clear();

            for (int day = 1; day <= 7; day++)
            {
                DBManager databaseManager = new DBManager();

                int col = (day - 1) % 7;

                DateTime dummyDate = firstDayOfWeek.AddDays(day - 1);

                var tasksForDate = databaseManager.RetrieveEventDataByDate(dummyDate);
                string taskNames = string.Join("\n", tasksForDate.Select(t => t.name));

                Button dateLabel = new Button
                {
                    Content = dummyDate.Day.ToString(),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.Gray,
                    Foreground = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };

                if (dummyDate == DateTime.Today)
                {
                    dateLabel.Foreground = Brushes.Black;
                    dateLabel.BorderBrush = Brushes.Black;
                    dateLabel.BorderThickness = new Thickness(2);
                }

                dateLabel.Content += "\n" + taskNames;

                CalendarGrid.Children.Add(dateLabel);
                Grid.SetColumn(dateLabel, col);
            }
        }
    }
}
