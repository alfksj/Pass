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
                fileOpened = "null";
            }
            Setting.load();
            const string lang = "en-US";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            InitializeComponent();
            addicSup.Visibility = Visibility.Hidden;
            internet = new Internet(rm, progress);
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
            internet.serverStart();
        }
        private int keptKeyDown = 0;
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
                    if (retries >= 2)
                    {
                        addicSup.Dispatcher.Invoke(() =>
                        {
                            addicSup.Visibility = Visibility.Visible;
                        });
                    }
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

        private void killer(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void exec(int whoareu, Label label)
        {
            if (fileOpened.Equals("null"))
            {
                MessageBox.Show(rm.GetString("nosel"), "Pass");
                return;
            }
            string ip = IP[whoareu];
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
                label.Dispatcher.Invoke(() =>
                {
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
