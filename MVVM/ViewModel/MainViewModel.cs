using DiskoAIO.Core;
using DiskoAIO.MVVM.View;
using DiskoAIO.Properties;
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
        public RelayCommand KryptoViewCommand { get; set; }
        public RelayCommand PresenceViewCommand { get; set; }
        public RelayCommand SniperViewCommand { get; set; }
        public RelayCommand TwitterViewCommand { get; set; }
        public RelayCommand TwitterAccountsViewCommand { get; set; }
        public RelayCommand PremintViewCommand { get; set; }
        public RelayCommand PremintAccountsViewCommand { get; set; }

        public RelayCommand SettingsViewCommand { get; set; }
        //Window change commands
        public JoinerView JoinerView { get; set; } = new JoinerView();
        public GiveawayView GiveawaysView { get; set; } = new GiveawayView();
        public ProxiesView ProxiesView { get; set; } = new ProxiesView();
        public AccountsView AccountsView { get; set; } = new AccountsView();
        public TasksView TasksView { get; set; } = new TasksView();
        public ChatView ChatView { get; set; } = new ChatView();
        public KryptoView KryptoView { get; set; } = new KryptoView();
        public PresenceView PresenceView { get; set; } = new PresenceView();
        public SniperView SniperView { get; set; } = new SniperView();
        public TwitterView TwitterView { get; set; } = new TwitterView();
        public TwitterAccountsView TwitterAccountsView { get; set; } = new TwitterAccountsView();
        public PremintView PremintView { get; set; } = new PremintView();
        public PremintAccountsView PremintAccountsView { get; set; } = new PremintAccountsView();
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
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                JoinerView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                JoinerView.TokenGroup.ItemsSource = source;
            });
            GiveawaysViewCommand = new RelayCommand(o =>
            {
                CurrentView = GiveawaysView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                GiveawaysView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                GiveawaysView.TokenGroup.ItemsSource = source;
            });
            ProxiesViewCommand = new RelayCommand(o =>
            {
                CurrentView = ProxiesView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                ProxiesView.GroupComboBox.ItemsSource = source;
                if (App.proxyGroups.Count > 0)
                    ProxiesView.ListProxies.SelectedItem = App.proxyGroups.First()._name;
                ProxiesView.ListProxies.SelectedItem = Settings.Default.ProxyGroup;
            });
            AccountsViewCommand = new RelayCommand(o =>
            {
                CurrentView = AccountsView;
                var source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                AccountsView.GroupComboBox.ItemsSource = source;
                if (App.accountsGroups.Count > 0)
                    AccountsView.ListTokens.SelectedItem = App.accountsGroups.First()._name;
                AccountsView.ListTokens.SelectedItem = Settings.Default.TokenGroup;
            });
            TasksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TasksView;
            });
            ChatViewCommand = new RelayCommand(o => {
                CurrentView = ChatView;
                var source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                ChatView.TokenGroup.ItemsSource = source;
            });
            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                SettingsView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                SettingsView.TokenGroup.ItemsSource = source;
            });
            KryptoViewCommand = new RelayCommand(o =>
            {
                CurrentView = KryptoView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                KryptoView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                KryptoView.TokenGroup.ItemsSource = source;
            });
            PresenceViewCommand = new RelayCommand(o =>
            {
                CurrentView = PresenceView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                PresenceView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                PresenceView.TokenGroup.ItemsSource = source;
            });
            SniperViewCommand = new RelayCommand(o =>
            {
                CurrentView = SniperView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                SniperView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.accountsGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                SniperView.TokenGroup.ItemsSource = source;
            });
            TwitterViewCommand = new RelayCommand(o =>
            {
                CurrentView = TwitterView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                TwitterView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.twitterGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                TwitterView.TokenGroup.ItemsSource = source;
            });
            TwitterAccountsViewCommand = new RelayCommand(o =>
            {
                CurrentView = TwitterAccountsView;
                var source = new string[] { };
                foreach (var group in App.twitterGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                TwitterAccountsView.GroupComboBox.ItemsSource = source;
                if (App.twitterGroups.Count > 0)
                    TwitterAccountsView.ListTokens.SelectedItem = App.twitterGroups.First()._name;
                TwitterAccountsView.ListTokens.SelectedItem = Settings.Default.TwitterGroup;
            });
            PremintViewCommand = new RelayCommand(o =>
            {
                CurrentView = PremintView;
                var source = new string[] { };
                foreach (var group in App.proxyGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                PremintView.ProxiesGroup.ItemsSource = source;
                source = new string[] { };
                foreach (var group in App.premintGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                PremintView.TokenGroup.ItemsSource = source;
            });
            PremintAccountsViewCommand = new RelayCommand(o =>
            {
                CurrentView = PremintAccountsView;
                var source = new string[] { };
                foreach (var group in App.premintGroups)
                {
                    source = source.Append(group._name).ToArray();
                }
                PremintAccountsView.GroupComboBox.ItemsSource = source;
                if (App.premintGroups.Count > 0)
                    PremintAccountsView.ListTokens.SelectedItem = App.premintGroups.First()._name;
            });
        }
    }
}
