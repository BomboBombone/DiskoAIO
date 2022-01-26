using DiskoAIO.Core;
using DiskoAIO.MVVM.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand JoinerViewCommand { get; set; }
        public RelayCommand GiveawaysViewCommand { get; set; }
        public RelayCommand TasksViewCommand { get; set; }
        public RelayCommand ProxiesViewCommand { get; set; }
        public RelayCommand AccountsViewCommand { get; set; }
        public RelayCommand ChatViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }
        //Window change commands
        public JoinerView JoinerView { get; set; } = new JoinerView();
        public GiveawayView GiveawaysView { get; set; } = new GiveawayView();
        public ProxiesView ProxiesView { get; set; } = new ProxiesView();
        public AccountsView AccountsView { get; set; } = new AccountsView();
        public TasksView TasksView { get; set; } = new TasksView();
        public ChatView ChatView { get; set; } = new ChatView();
        public SettingsView SettingsView { get; set; } = new SettingsView();

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            if (App.taskManager == null)
                App.taskManager = new TasksView();

            CurrentView = JoinerView;

            JoinerViewCommand = new RelayCommand(o =>
            {
                CurrentView = JoinerView;
            });
            GiveawaysViewCommand = new RelayCommand(o =>
            {
                CurrentView = GiveawaysView;
            });
            ProxiesViewCommand = new RelayCommand(o =>
            {
                CurrentView = ProxiesView;
            });
            AccountsViewCommand = new RelayCommand(o =>
            {
                CurrentView = AccountsView;
            });
            TasksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TasksView;
            });
            ChatViewCommand = new RelayCommand(o => {
                CurrentView = ChatView;
            });
            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsView;
            });
        }
    }
}
