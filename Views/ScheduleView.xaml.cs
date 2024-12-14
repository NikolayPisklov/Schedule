using Schedule.Command;
using Schedule.Models.CombinedModels;
using Schedule.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Schedule.Views
{
    /// <summary>
    /// Interaction logic for Schedule.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        
        public ScheduleView()
        {
            InitializeComponent();
            this.Loaded += ViewLoaded;
            Messanger.Instance.ScheduleDone += OnScheduleDone;
        }
        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;
            //Add columns
            if (viewModel != null)
            {
                var classCount = viewModel.SchedulesInfo.Count;
                for (int i = 0; i < classCount; i++)
                {
                    GridLength width = new GridLength(1, GridUnitType.Star);
                    scheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
                }   
                SetClassesTitleAndButtons(viewModel);
                PaintBorders();
            }           
        }
        
        //View first loading
        private void SetClassesTitleAndButtons(ScheduleViewModel viewModel)
        {
            var count = viewModel.ClassesCount;
            for (int i = 0; i < count; i++)
            {
                TextBlock textBlock = CreateClassTitleTextblock(viewModel, i);
                Button button = CreateButtonForClass(viewModel, i);
                StackPanel stackPanel = CreateStackPanelForGridHead(button, textBlock);
                Grid.SetRow(stackPanel, 0);
                Grid.SetColumn(stackPanel, i + 1);
                scheduleGrid.Children.Add(stackPanel);
            }
        }
        private Button CreateButtonForClass(ScheduleViewModel viewModel,int elementNumber)
        {
            Button button = new Button();
            button.Content = $"Назначені предмети";
            // Установка привязки к команде
            var command = viewModel?.OpenWindowForAssigningSubjectsCommand;
            if (command != null)
            {
                button.Command = command;
            }
            // Установка привязки параметра команды
            var binding = new Binding($"SchedulesInfo[{elementNumber}]") { Source = this.DataContext };
            button.SetBinding(Button.CommandParameterProperty, binding);
            return button;
        }
        private TextBlock CreateClassTitleTextblock(ScheduleViewModel viewModel, int elementNumber) 
        {
            TextBlock tb = new TextBlock();
            tb.Text = $"{viewModel.SchedulesInfo[elementNumber].ClassTitle}";
            return tb;
        }
        private StackPanel CreateStackPanelForGridHead(Button btn, TextBlock tb) 
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            stackPanel.Children.Add(tb);
            stackPanel.Children.Add(btn);
            return stackPanel;
        }
        private void PaintBorders() 
        {
            //Painting borders
            for (int row = 0; row < scheduleGrid.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < scheduleGrid.ColumnDefinitions.Count; col++)
                {
                    Border border = new Border()
                    {
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = Brushes.Black
                    };
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    scheduleGrid.Children.Add(border);
                }
            }
        }
        //Schedule work
        public void OnScheduleDone() 
        {
            var viewModel = this.DataContext as ScheduleViewModel;
            var slots = viewModel.SlotsForTheView;
            for(int c = 0; c < viewModel.ClassesCount; c++) 
            {
                for(int i = 1; i <= viewModel.DaysIds.Count; i++) 
                {
                    ListView newListView = new ListView();
                    for (int j = 1; j <= viewModel.TimeIds.Count; j++)
                    {
                        var slot = slots.FirstOrDefault(x => x.TimeId == j && x.DayId == i && x.ClassId == viewModel.SchedulesInfo[c].ClassId);//how to determine column?
                        if(slot is not null) 
                        {
                            newListView.Items.Add($"{slot.SubjectTitle}-{slot.FullName}");
                        }
                    }
                    Grid.SetRow(newListView, i);
                    Grid.SetColumn(newListView, c);
                    scheduleGrid.Children.Add(newListView);
                }
            }
        }
    }
}
