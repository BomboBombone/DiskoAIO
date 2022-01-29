using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for TasksView.xaml
    /// </summary>
    public partial class TasksView : UserControl
    {
        public static List<DiskoTask> tasks { get; set; } = new List<DiskoTask>();
        public Timer refreshUI { get; set; } = null;
        public TasksView()
        {
            InitializeComponent();
            ListTasks.ItemsSource = tasks;
            if (App.taskManager == null)
                App.taskManager = this;
            else
            {
                ListTasks.ItemsSource = tasks;
                ListTasks.Items.Refresh();
                App.taskManager = this;
            }
            if(refreshUI == null)
            {
                refreshUI = new System.Timers.Timer();
                refreshUI.Elapsed += new ElapsedEventHandler(OnElapsedTime);
                refreshUI.Interval = 1000;
                refreshUI.Enabled = true;
            }
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ListTasks.Items.Refresh();
            });
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTasks.ItemContainerGenerator.IndexFromContainer(lbItem);
            if (tasks[index].Running)
            {
                App.mainWindow.ShowNotification("Task is already running");
                return;
            }
            if(tasks[index].progress.completed_tokens == tasks[index].progress.total_tokens && !tasks[index].Running)
            {
                App.mainWindow.ShowNotification("Task has finished executing, cannot be resumed");
                return;
            }
            tasks[index].Resume();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTasks.ItemContainerGenerator.IndexFromContainer(lbItem);
            tasks[index].Pause();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTasks.ItemContainerGenerator.IndexFromContainer(lbItem);
            tasks[index].Stop();
            tasks.RemoveAt(index);
            ListTasks.Items.Refresh();
            App.mainWindow.ShowNotification("Successfully deleted task");
        }
        public void AddTask(DiskoTask task)
        {
            tasks.Add(task);
            tasks.Reverse();
            ListTasks.ItemsSource = tasks;
            ListTasks.Items.Refresh();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTasks.ItemContainerGenerator.IndexFromContainer(lbItem);
            if (tasks[index].Type == "Chat bot")
            {
                var popup = new ChatTaskSettingsPopup(((ChatBotTask)tasks[index]).serverID, ((ChatBotTask)tasks[index]).channelID, ((ChatBotTask)tasks[index]).userID, ((ChatBotTask)tasks[index]).lvlChannelID, ((ChatBotTask)tasks[index]).maxLvl);
                popup.Show();
            }
            else
            {
                var currTask = tasks[index];
                var popup = new TaskSettingsPopup(currTask.accountGroup._name, currTask.proxyGroup == null ? "None" : currTask.proxyGroup._name, true, "Delay (ms)", currTask.delay.ToString());
                popup.Show();
            }
        }
    }
}
