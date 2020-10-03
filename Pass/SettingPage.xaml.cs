using PassLibrary;
using PassLibrary.NetworkHistory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

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

        public void Active(Grid target, RoutedEventArgs e)
        {
            content.Content = rm.GetString(target.Tag.ToString());
            target.SetValue(Grid.ColumnProperty, 1);
            target.Margin = visibleMargin;
            target.Visibility = Visibility.Visible;
            currentOn.Visibility = Visibility.Hidden;
            currentOn = target;
            network_btn.IsEnabled = true;
            general_btn.IsEnabled = true;
            block_btn.IsEnabled = true;
            about_btn.IsEnabled = true;
            securiry_btn.IsEnabled = true;
            ((Button)e.Source).IsEnabled = false;
        }
        public void Save(object sender, CancelEventArgs e)
        {
            PushSetting();
            Setting.Save();
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
        private void Network_btn_Click(object sender, RoutedEventArgs e)
        {
            Active(network, e);
        }
        private void General_btn_Click(object sender, RoutedEventArgs e)
        {
            Active(general, e);
        }
        private void About_btn_Click(object sender, RoutedEventArgs e)
        {
            Active(about, e);
        }
        private void Security_btn_Click(object sender, RoutedEventArgs e)
        {
            Active(security, e);
        }
        private void Block_btn_Click(object sender, RoutedEventArgs e)
        {
            //read db and apply.
            List<Record> records = Internet.history.ReadHistory();
            Internet.history.DeleteAll();
            records.ForEach((cons) =>
            {
                history.Items.Add(new HistoryRow(cons));
            });
            Active(netHistory, e);
        }
        private class HistoryRow
        {
            public string IP { get; set; }
            public string MAC { get; set; }
            public string TYPE { get; set; }
            public string TIME { get; set; }
            public string RESPONSE { get; set; }
            public string FILE { get; set; }
            public string RESULT { get; set; }
            public HistoryRow(Record record)
            {
                IP = record.Ip;
                MAC = record.Mac;
                TYPE = NetworkHistory.EnumToString(record.Type);
                TIME = record.time.ToString("yyyy/MM/dd HH:mm:ss.fff");
                JObject obj = JObject.Parse(record.Comment);
                //JSON FORMAT:
                //ping:{"reponse":"accept/versMis/local/deny/stealth"}
                //pingReply:{"reply":"accept/versMis/local/deny"}
                //incomming_share:{"file":foo","status":"vers/sucess/invalid_msg/hash_mismatch/file_error/deny/user_deny"}
                //outgoing_share:{"file":"foo","status":"VersionMismatch/notSharing/sucess/invalid_msg/file_error/user_deny/hash_mismatch"}
                //TODO: Test all new feature that they work as expected
                if(record.Type == COMMU_TYPE.PING)
                {
                    RESPONSE = obj.Value<string>("response");
                }
                else if (record.Type == COMMU_TYPE.PING_REPLY)
                {
                    RESPONSE = obj.Value<string>("reply");
                }
                else if (record.Type == COMMU_TYPE.INCOMMING_SHARE)
                {
                    FILE = obj.Value<string>("file");
                    RESULT = obj.Value<string>("status");
                }
                else if (record.Type == COMMU_TYPE.OUTGOING_SHARE)
                {
                    FILE = obj.Value<string>("file");
                    RESULT = obj.Value<string>("status");
                }
                else
                {
                    RESULT = "UNKNOWN COMMUNICATION TYPE";
                }
            }
        }
        private readonly Dictionary<string, string> languageTable = new Dictionary<string, string>();
        private readonly Dictionary<string, int> languageISO = new Dictionary<string, int>();
        public void ApplySetting()
        {
            Log.log("Applying to UI");
            sharing.isToggled = Setting.Sharing;
            askBeforeShare.isToggled = Setting.AskBeforeShare;
            name_setting.Text = Setting.Name;
            name.Content = Setting.Name;
            downloadPath.Content = Setting.DefaultSave;
            ipAddr.Content = Internet.GetLocalIPAddress();
            defSavingPath.Text = Setting.DefaultSave;
            PingTimeout.Value = Setting.PingTimeout;
            timeMs.Content = Setting.PingTimeout+"ms";
            statusVisible.isToggled = Setting.StatusOnLaunch;
            openLogPage.isToggled = Setting.LogOnLaunch;
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
            autoScanOnBoot.isToggled = Setting.AutoStartOnBoot;
            AutoScanOnLaunch.isToggled = Setting.AutoScanOnLaunch;
            autoStartOnBoot.isToggled = Setting.AutoStartOnBoot;
            lang.SelectedIndex = languageISO[Setting.Language];
            StealthMode.isToggled = Setting.StealthMode;
            aes_iv.Text = Setting.Aes_Initial_Value;
            Log.log("Applied");
        }
        public void PushSetting()
        {
            Log.log("Pushing data to Setting");
            Setting.Sharing = sharing.isToggled;
            Setting.Name = name_setting.Text;
            Setting.AskBeforeShare = askBeforeShare.isToggled;
            Setting.DefaultSave = defSavingPath.Text;
            Setting.PingTimeout = (int)PingTimeout.Value;
            Setting.StatusOnLaunch = statusVisible.isToggled;
            Setting.LogOnLaunch = openLogPage.isToggled;
            Setting.AutoStartOnBoot = autoScanOnBoot.isToggled;
            Setting.AutoScanOnLaunch = AutoScanOnLaunch.isToggled;
            Setting.AutoStartOnBoot = autoStartOnBoot.isToggled;
            Setting.Language = languageTable[((ComboBoxItem)lang.SelectedItem).Content.ToString()];
            Setting.StealthMode = StealthMode.isToggled;
            Setting.Aes_Initial_Value = aes_iv.Text;
            Log.log("Pushed");
        }
        private void PingTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeMs.Content = (int)PingTimeout.Value+"ms";
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(rm.GetString("TryingToReset"), "Pass", MessageBoxButton.YesNo);
            if(res==MessageBoxResult.No)
            {
                return;
            }
            Setting.ClearEverything();
            Setting.Load();
            ApplySetting();
        }

        public string Originated { get; set; }
        public string[] Arguments { get; set; }
        private void RestartPass_Click(object sender, RoutedEventArgs e)
        {
            //TODO: implement this func
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult x = MessageBox.Show(rm.GetString("deleteHistoryWarning"), "Pass", MessageBoxButton.YesNo);
            if(x == MessageBoxResult.Yes)
            {
                Console.WriteLine("DELETE!");
                Internet.history.DeleteAll();
                history.Items.Clear();
                List<Record> records= Internet.history.ReadHistory();
                records.ForEach((cons) =>
                {
                    history.Items.Add(new HistoryRow(cons));
                });
            }
        }
    }
}
