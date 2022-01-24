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
        public JoinerVM JoinerVm { get; set; } = new JoinerVM();
        public GiveawaysVM GiveawaysVm { get; set; } = new GiveawaysVM();
        public ProxiesVM ProxiesVm { get; set; } = new ProxiesVM();
        public AccountsVM AccountsVm { get; set; } = new AccountsVM();
        public TasksVM TasksVm { get; set; } = new TasksVM();
        public ChatVM ChatVm { get; set; } = new ChatVM();
        public SettingsVM SettingsVm { get; set; } = new SettingsVM();

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

            CurrentView = JoinerVm;

            JoinerViewCommand = new RelayCommand(o =>
            {
                CurrentView = JoinerVm;
            });
            GiveawaysViewCommand = new RelayCommand(o =>
            {
                CurrentView = GiveawaysVm;
            });
            ProxiesViewCommand = new RelayCommand(o =>
            {
                CurrentView = ProxiesVm;
            });
            AccountsViewCommand = new RelayCommand(o =>
            {
                CurrentView = AccountsVm;
            });
            TasksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TasksVm;
            });
            ChatViewCommand = new RelayCommand(o => {
                CurrentView = ChatVm;
            });
            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVm;
            });
        }
    }
}
