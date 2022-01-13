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
        public RelayCommand SettingsViewCommand { get; set; }
        //Window change commands
        public JoinerVM JoinerVm { get; set; }
        public GiveawaysVM GiveawaysVm { get; set; }
        public ProxiesVM ProxiesVm { get; set; }
        public AccountsVM AccountsVm { get; set; }
        public TasksVM TasksVm { get; set; }
        public SettingsVM SettingsVm { get; set; }

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
            JoinerVm = new JoinerVM();
            GiveawaysVm = new GiveawaysVM();
            ProxiesVm = new ProxiesVM();
            AccountsVm = new AccountsVM();
            TasksVm = new TasksVM();
            SettingsVm = new SettingsVM();

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
            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVm;
            });
        }
    }
}
