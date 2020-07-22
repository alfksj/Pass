using PassLibrary;
using System.ComponentModel;
using System.Resources;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;

namespace Pass
{
    /// <summary>
    /// Interaction logic for SettingPage.xaml
    /// </summary>
    public partial class SettingPage : Window
    {
        private Grid currentOn;
        private ResourceManager rm;
        private Thickness visibleMargin = new Thickness(0, 56.553, 10, 10);

        public ResourceManager Rm { set => rm = value; }

        public void active(Grid target, RoutedEventArgs e)
        {
            content.Content = rm.GetString(target.Tag.ToString());
            target.SetValue(Grid.ColumnProperty, 1);
            target.Margin = visibleMargin;
            target.Visibility = Visibility.Visible;
            currentOn.Visibility = Visibility.Hidden;
            currentOn = target;
            network_btn.IsEnabled = true;
            general_btn.IsEnabled = true;
            about_btn.IsEnabled = true;
            ((Button)e.Source).IsEnabled = false;
        }
        public void save(object sender, CancelEventArgs e)
        {
            pushSetting();
            Setting.save();
        }
        public SettingPage()
        {
            InitializeComponent();
            currentOn = network;
            
        }
        private void network_btn_Click(object sender, RoutedEventArgs e)
        {
            active(network, e);
        }
        private void general_btn_Click(object sender, RoutedEventArgs e)
        {
            active(general, e);
        }
        private void about_btn_Click(object sender, RoutedEventArgs e)
        {
            active(about, e);
        }
        public void applySetting()
        {
            Log.log("Applying to UI");
            sharing.IsToggled = Setting.sharing;
            askBeforeShare.IsToggled = Setting.askBeforeShare;
            name_setting.Text = Setting.name;
            name.Content = Setting.name;
            downloadPath.Content = Setting.defaultSave;
            ipAddr.Content = Internet.GetLocalIPAddress();
            defSavingPath.Text = Setting.defaultSave;
            PingTimeout.Value = Setting.pingTimeout;
            timeMs.Content = Setting.pingTimeout;
            statusVisible.IsToggled = Setting.statusOnLaunch;
            openLogPage.IsToggled = Setting.logOnLaunch;
            Log.log("Applied");
        }
        public void pushSetting()
        {
            Log.log("Pushing data to Setting");
            Setting.sharing = sharing.IsToggled;
            Setting.name = name_setting.Text;
            Setting.askBeforeShare = askBeforeShare.IsToggled;
            Setting.defaultSave = defSavingPath.Text;
            Setting.pingTimeout = (int)PingTimeout.Value;
            Setting.statusOnLaunch = statusVisible.IsToggled;
            Setting.logOnLaunch = openLogPage.IsToggled;
            Log.log("Pushed");
        }

        private void PingTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeMs.Content = (int)PingTimeout.Value+"ms";
        }
    }
}
