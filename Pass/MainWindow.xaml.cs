using System.Globalization;
using System.Collections.Generic;
using System.Resources;
using System.Threading;
using System.Windows;
using PassLibrary;
using System.Threading.Tasks;
using System;
using System.Windows.Input;
using Label = System.Windows.Controls.Label;
using Brushes = System.Windows.Media.Brushes;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Diagnostics;

namespace Pass
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private Visibility hide = Visibility.Hidden;
        private Visibility show = Visibility.Visible;
        private string fileOpened;
        private List<string> IP;
        private ResourceManager rm = new ResourceManager("Pass.Localization", typeof(MainWindow).Assembly);
        private Internet internet;
        public MainWindow()
        {
            string[] cmds = Environment.GetCommandLineArgs();
            if(cmds.Length>1)
            {
                fileOpened = cmds[1];
            }
            else
            {
                fileOpened = null;
            }
            Setting.checkEnvironment();
            Setting.load();
            const string lang = "en-US";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            InitializeComponent();
            SettingPage settingPage = new SettingPage();
            settingPage.Rm = rm;
            settingPage.applySetting();
            if (Setting.logOnLaunch) debuger.Show();
            if (Setting.statusOnLaunch) status.Visibility = show;
            //DEBUG//
            //Hide();
            settingPage.Show();
            /////////
            //TRAY
            NotifyIcon ni = new NotifyIcon();
            ContextMenu tray = new ContextMenu();
            MenuItem exit = new MenuItem();
            exit.Index = 0;
            exit.Text = rm.GetString("exit");
            exit.Click += delegate (object click, EventArgs e)
            {
                Environment.Exit(0);
            };
            MenuItem open = new MenuItem();
            open.Text = rm.GetString("open");
            open.Click += delegate (object click, EventArgs e)
            {
                Show();
            };
            MenuItem setting = new MenuItem();
            setting.Text = rm.GetString("settings");
            setting.Click += delegate (object click, EventArgs e)
            {
                settingPage.Show();
            };
            open.Index = 0;
            tray.MenuItems.Add(setting);
            tray.MenuItems.Add(open);
            tray.MenuItems.Add(exit);
            ResourceManager resource = new ResourceManager("Pass.Resource", typeof(MainWindow).Assembly);
            ni.Icon = new Icon("assets/pass.ico");
            ni.Visible = true;
            ni.DoubleClick += delegate (object senders, EventArgs e)
            {
                Show();
            };
            ni.ContextMenu = tray;
            ni.Text = "Pass";
            //
            if (fileOpened != null)
            {
                statusText.Content = fileOpened;
            }
            else statusText.Content = "Not selelcted";
            internet = new Internet(rm);
            internet.setFunction(
            (bool action) =>
            {
                progress.Dispatcher.Invoke(() =>
                {
                    progress.IsIndeterminate = action;
                });
            },
            (int max) =>
            {
                progress.Dispatcher.Invoke(() =>
                {
                    progress.IsIndeterminate = false;
                    progress.Minimum = 0;
                    progress.Maximum = max;
                    progress.Value = 0;
                });
            },
            () =>
            {
                progress.Dispatcher.Invoke(() =>
                {
                    progress.Value++;
                });
            },
            (bool show) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (show) Show();
                    else Hide();
                });
            });
            Task.Run(() =>
            {
                internet.PingReceiver();
            });
            MyIp.Content = rm.GetString("mypc") + ": " + internet.myIP;
            q.Visibility = hide;
            w.Visibility = hide;
            e.Visibility = hide;
            r.Visibility = hide;
            t.Visibility = hide;
            y.Visibility = hide;
            u.Visibility = hide;
            lst.Visibility = hide;
            sup.setResourceManager(rm);
            debuger.setResourceManager(rm);
            internet.serverStart(this);
        }
        private int keptKeyDown = 0;
        private long lastKeyDown=0;
        private Debuger debuger = new Debuger();
        private void keyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.F9)
            {
                keptKeyDown++;
            }
            else
            {
                keptKeyDown = 0;
            }
            if(e.Key==Key.LeftCtrl)
            {
                if(lastKeyDown==0)
                {
                    lastKeyDown = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                else
                {
                    long interval=(long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds-lastKeyDown;
                    if(interval<=1)
                    {
                        status.Visibility = status.Visibility == show ? hide : show;
                    }
                    lastKeyDown = 0;
                }
            }
            if(keptKeyDown==5)
            {
                Log.log("Debugger opened");
                debuger.Show();
            }
        }
        int retries=0;
        private void refresh_Click(object sender, RoutedEventArgs ex)
        {
            refresh.IsEnabled = false;
            none.Visibility = Visibility.Hidden;
            finding.Visibility = Visibility.Visible;
            lst.Visibility = Visibility.Hidden;
            Task.Run(() =>
            {
                List<string> ip = internet.scanLocalIp();
                finding.Dispatcher.Invoke(() =>
                {
                    finding.Visibility = Visibility.Hidden;
                });
                if (ip.Count == 0)
                {
                    none.Dispatcher.Invoke(() =>
                    {
                        none.Visibility = Visibility.Visible;
                    });
                    retries++;
                }
                else
                {
                    retries = 0;
                    lst.Dispatcher.Invoke(() =>
                    {
                        lst.Visibility = show;
                    });
                    int queue = 0;
                    IP = ip;
                    q.Dispatcher.Invoke(() =>
                    {
                        ip.ForEach((string addr) =>
                        {
                            switch (queue)
                            {
                                case 0:
                                    q.Visibility = show;
                                    ip1.Content = addr;
                                    break;
                                case 1:
                                    w.Visibility = show;
                                    ip2.Content = addr;
                                    break;
                                case 2:
                                    e.Visibility = show;
                                    ip3.Content = addr;
                                    break;
                                case 3:
                                    r.Visibility = show;
                                    ip4.Content = addr;
                                    break;
                                case 4:
                                    t.Visibility = show;
                                    ip5.Content = addr;
                                    break;
                                case 5:
                                    y.Visibility = show;
                                    ip6.Content = addr;
                                    break;
                                case 6:
                                    u.Visibility = show;
                                    ip7.Content = addr;
                                    break;
                            }
                            queue++;
                        });
                    });
                }
                debuger.setReponse(internet.lastResponse);
                refresh.Dispatcher.Invoke(() =>
                {
                    refresh.IsEnabled = true;
                });
                return;
            });
        }

        private Support sup = new Support();
        private void sup2_Click(object sender, RoutedEventArgs e)
        {
            sup.go(Support.NONE_VISIBLE);
        }
        private void sup1_Click(object sender, RoutedEventArgs e)
        {
            sup.go(Support.NONE_VISIBLE);
        }

        private void killer(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void exec(int whoareu, Label label)
        {
            if(!Setting.sharing)
            {
                MessageBox.Show(rm.GetString("sharingAlert"), "Pass", MessageBoxButtons.OK);
                return;
            }
            if (fileOpened.Equals("null"))
            {
                MessageBox.Show(rm.GetString("nosel"), "Pass");
                return;
            }
            if (Setting.askBeforeShare)
            {
                DialogResult opti = MessageBox.Show(rm.GetString("askBeforeShare1")+fileOpened+rm.GetString("askBeforeShare2"), "Pass", MessageBoxButtons.YesNo);
                if (opti == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
            }
            string ip = IP[whoareu];
            string origin = label.Content.ToString();
            label.FontWeight = FontWeights.Bold;
            label.Content = rm.GetString("sending");
            label.Foreground = Brushes.LawnGreen;
            Task.Run(()=>{
                int result = internet.wannaSendTo(ip, fileOpened);
                if(result==Internet.DENIED)
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        label.Content = rm.GetString("denied");
                        label.Foreground = Brushes.Red;
                    });
                }
                else if(result==Internet.ERROR)
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        label.Content = rm.GetString("error");
                        label.Foreground = Brushes.Red;
                    });
                }
                else if(result==Internet.SUCESS)
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        label.Content = rm.GetString("sent");
                    });
                }
                Thread.Sleep(1000);
                label.Dispatcher.Invoke(() =>
                {
                    label.Content = origin;
                    label.Foreground = Brushes.White;
                    label.FontWeight = FontWeights.Normal;
                });
            });
        }

        private void q1_Click(object sender, RoutedEventArgs e)
        {
            exec(0, ip1);
        }

        private void q2_Click(object sender, RoutedEventArgs e)
        {
            exec(1, ip2);
        }

        private void q3_Click(object sender, RoutedEventArgs e)
        {
            exec(2, ip3);
        }

        private void q4_Click(object sender, RoutedEventArgs e)
        {
            exec(3, ip4);
        }

        private void q5_Click(object sender, RoutedEventArgs e)
        {
            exec(4, ip5);
        }

        private void q6_Click(object sender, RoutedEventArgs e)
        {
            exec(5, ip6);
        }

        private void q7_Click(object sender, RoutedEventArgs e)
        {
            exec(6, ip7);
        }
    }
}
