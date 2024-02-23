using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace MyCalendar
{
    public partial class DailyView : Page
    {
        private int _month { get; set; }
        private int _year { get; set; }
        private int _day { get; set; }
        private DateTime Today { get; set; }
        public DailyView()
        {
            InitializeComponent();

            DateTime today = DateTime.Today;
            Today = today;
            _month = today.Month;
            _year = today.Year;
            _day = today.Day;

            DayLabel.Content = Today.ToLongDateString();

            PopulateCalendarGrid();
        }

        private void PrevDay_Click(object sender, RoutedEventArgs e)
        {
            DateTime prevDay = new DateTime(_year, _month, _day);

            prevDay = prevDay.AddDays(-1);
            _month = prevDay.Month;
            _day = prevDay.Day;
            _year = prevDay.Year;

            DayLabel.Content = prevDay.ToLongDateString();
            PopulateCalendarGrid();
        }

        private void NextDay_Click(object sender, RoutedEventArgs e)
        {
            DateTime nextDay = new DateTime(_year, _month, _day);

            nextDay = nextDay.AddDays(1);
            _month = nextDay.Month;
            _day = nextDay.Day;
            _year = nextDay.Year;

            DayLabel.Content = nextDay.ToLongDateString();
            PopulateCalendarGrid();
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

            PopulateCalendarGrid();
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

            PopulateCalendarGrid();
        }

        private void PopulateCalendarGrid()
        {

            CalendarGridTitle.Visibility = Visibility.Visible;

            NoEventsLabel.Visibility = Visibility.Collapsed;

            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();

            DBManager databaseManager = new DBManager();
            DateTime dateTime = new DateTime(_year, _month, _day);
            var tasksForDate = databaseManager.RetrieveEventDataByDate(dateTime);

            if (tasksForDate.Count == 0)
            {

                CalendarGridTitle.Visibility = Visibility.Hidden;

                NoEventsLabel.Visibility = Visibility.Visible;

                return;
            }

            for (int j = 0; j < tasksForDate.Count; j++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            int i = 1;

            foreach (Event _event in tasksForDate)
            {
                Label timeLabel = new Label
                {
                    Content = _event.name,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.Gray,
                    Foreground = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };

                CalendarGrid.Children.Add(timeLabel);
                Grid.SetRow(timeLabel, i - 1);
                Grid.SetColumn(timeLabel, 0);

                Button taskLabel = new Button
                {
                    Content = _event.description,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.Gray,
                    Foreground = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };

                CalendarGrid.Children.Add(taskLabel);
                Grid.SetRow(taskLabel, i - 1);
                Grid.SetColumn(taskLabel, 1);

                i++;
            }
        }  
    }
}
