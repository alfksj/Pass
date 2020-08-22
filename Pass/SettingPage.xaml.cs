using PassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
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
            securiry_btn.IsEnabled = true;
            ((Button)e.Source).IsEnabled = false;
        }
        public void save(object sender, CancelEventArgs e)
        {
            pushSetting();
            Setting.save();
            Hide();
            e.Cancel = true;
        }
        public SettingPage()
        {
            InitializeComponent();
            languageTable["English"] = "en-US"; languageISO["en-US"] = 0;
            languageTable["Korean"] = "ko-KR";  languageISO["ko-KR"] = 1;
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
        private void security_btn_Click(object sender, RoutedEventArgs e)
        {
            active(security, e);
        }
        private Dictionary<string, string> languageTable = new Dictionary<string, string>();
        private Dictionary<string, int> languageISO = new Dictionary<string, int>();
        public void applySetting()
        {
            Log.log("Applying to UI");
            sharing.isToggled = Setting.sharing;
            askBeforeShare.isToggled = Setting.askBeforeShare;
            name_setting.Text = Setting.name;
            name.Content = Setting.name;
            downloadPath.Content = Setting.defaultSave;
            ipAddr.Content = Internet.GetLocalIPAddress();
            defSavingPath.Text = Setting.defaultSave;
            PingTimeout.Value = Setting.pingTimeout;
            timeMs.Content = Setting.pingTimeout+"ms";
            statusVisible.isToggled = Setting.statusOnLaunch;
            openLogPage.isToggled = Setting.logOnLaunch;
            string macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
            string[] partition = new string[6];
            for(int i=0;i<macAddress.Length;i++)
            {
                if ((i+1)%2==0)
                {
                    partition[(i - 1) / 2] = macAddress.Substring(i - 1, 2);
                }
            }
            macAddr.Content = string.Join(":", partition);
            autoScanOnBoot.isToggled = Setting.autoStartOnBoot;
            AutoScanOnLaunch.isToggled = Setting.autoScanOnLaunch;
            autoStartOnBoot.isToggled = Setting.autoStartOnBoot;
            lang.SelectedIndex = languageISO[Setting.language];
            StealthMode.isToggled = Setting.stealthMode;
            aes_iv.Text = Setting.Aes_Initial_Value;
            Log.log("Applied");
        }
        public void pushSetting()
        {
            Log.log("Pushing data to Setting");
            Setting.sharing = sharing.isToggled;
            Setting.name = name_setting.Text;
            Setting.askBeforeShare = askBeforeShare.isToggled;
            Setting.defaultSave = defSavingPath.Text;
            Setting.pingTimeout = (int)PingTimeout.Value;
            Setting.statusOnLaunch = statusVisible.isToggled;
            Setting.logOnLaunch = openLogPage.isToggled;
            Setting.autoStartOnBoot = autoScanOnBoot.isToggled;
            Setting.autoScanOnLaunch = AutoScanOnLaunch.isToggled;
            Setting.autoStartOnBoot = autoStartOnBoot.isToggled;
            Setting.language = languageTable[((ComboBoxItem)lang.SelectedItem).Content.ToString()];
            Setting.stealthMode = StealthMode.isToggled;
            Setting.Aes_Initial_Value = aes_iv.Text;
            Log.log("Pushed");
        }
        private void PingTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeMs.Content = (int)PingTimeout.Value+"ms";
        }

        private void resetSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(rm.GetString("TryingToReset"), "Pass", MessageBoxButton.YesNo);
            if(res==MessageBoxResult.No)
            {
                return;
            }
            Setting.clearEverything();
            Setting.load();
            applySetting();
        }

        public string originated { get; set; }
        public string[] arguments { get; set; }
        private void restartPass_Click(object sender, RoutedEventArgs e)
        {
            //TODO: implement this func
        }
    }
}
