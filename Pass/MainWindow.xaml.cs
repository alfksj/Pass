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
using MenuItem = System.Windows.Forms.MenuItem;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Application = System.Windows.Application;
using System.IO;

namespace Pass
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private const Visibility hide = Visibility.Hidden;
        private const Visibility show = Visibility.Visible;
        private readonly string fileOpened = null;
        public string originated = null;

        private List<string> IP;
        private readonly ResourceManager rm = new ResourceManager("Pass.Localization", typeof(MainWindow).Assembly);
        private readonly Internet internet = null;
        public static Window Me_Myself;

        public bool debugging = false;
        private readonly bool dontExitOnClosing = true;
        private readonly bool exitOnDone = false;
        /********************************************************************
        //ALERT//
        private Thickness up = new Thickness(180, -144, 180, 0);
        private Thickness down = new Thickness(180, 0, 180, 0);
        public const int ALERT_OK_MESSAGE = 0, ALERT_YES_NO_MESSAGE = 1, ALERT_OK_CANCEL_MESSAGE = 2, ANIMATION_TIME = 200;
        public int showAlert(string msg, string title, int type)
        {
            titleF.Content = title;
            message.Text = msg;
            if(type==ALERT_OK_CANCEL_MESSAGE)
            {
                positive.Content = "OK";
                negative.Content = "CANCEL";
            }
            else if(type==ALERT_YES_NO_MESSAGE)
            {
                positive.Content = "YES";
                negative.Content = "NO";
            }
            else if(type==ALERT_OK_MESSAGE)
            {
                positive.Content = "OK";
            }

            var fade = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME)
            };
            var loc = new ThicknessAnimation()
            {
                From = up,
                To = down,
                Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME)
            };
            Storyboard.SetTarget(fade, alert);
            Storyboard.SetTarget(loc, alert);
            Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(loc, new PropertyPath(MarginProperty));
            var sb = new Storyboard();
            sb.Children.Add(fade);
            sb.Children.Add(loc);
            sb.Begin();
            return 0;
        }
        private void removeAlert()
        {
            var fadex = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME)
            };
            var locx = new ThicknessAnimation()
            {
                From = down,
                To = up,
                Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME)
            };
            Storyboard.SetTarget(fadex, alert);
            Storyboard.SetTarget(locx, alert);
            Storyboard.SetTargetProperty(fadex, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(locx, new PropertyPath(MarginProperty));
            var sbx = new Storyboard();
            sbx.Children.Add(fadex);
            sbx.Children.Add(locx);
            sbx.Begin();
        }
        private void positive_Click(object sender, RoutedEventArgs e)
        {
            Me_Myself.Dispatcher.Invoke(() =>
            {
                removeAlert();
            });
        }
        private void negative_Click(object sender, RoutedEventArgs e)
        {
            removeAlert();
        }
        //
        ********************************************************************/
        public MainWindow()
        {
            Me_Myself = this;
            string[] cmds = Environment.GetCommandLineArgs();
            Setting.CheckEnvironment();
            Setting.Load();
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Setting.Language);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Setting.Language);
            SettingPage settingPage = new SettingPage();
            InitializeComponent();
            settingPage.Rm = rm;
            settingPage.Originated = originated;
            settingPage.Arguments = cmds;
            settingPage.ApplySetting();
            status.Visibility = hide;
            if (Setting.LogOnLaunch) debuger.Show();
            if (Setting.StatusOnLaunch) status.Visibility = show;
            if (fileOpened != null)
            {
                statusText.Content = fileOpened;
            }
            else statusText.Content = "Not selelcted";
            //netwoek history
            PassLibrary.NetworkHistory.NetworkHistory networkHistory = new PassLibrary.NetworkHistory.NetworkHistory();
            Internet.history = networkHistory;
            PassLibrary.NetworkHistory.NetworkHistory.Rm = rm;
            networkHistory.OpenHistory();
            //
            internet = new Internet();
            internet.SetFunction(
            (bool action) =>
            {
                try {
                    progress.Dispatcher.Invoke(() =>
                    {
                        if (!IsVisible) return;
                        progress.IsIndeterminate = action;
                    });
                } catch (Exception e)
                {
                    Log.log(e.Message, Log.WARN);
                }
            },
            (int max) =>
            {
                try
                {
                    progress.Dispatcher.Invoke(() =>
                    {
                        if (!IsVisible) return;
                        progress.IsIndeterminate = false;
                        progress.Minimum = 0;
                        progress.Maximum = max;
                        progress.Value = 0;
                    });
                } catch (Exception e)
                {
                    Log.log(e.Message, Log.WARN);
                }
            },
            () =>
            {
                try
                {
                    progress.Dispatcher.Invoke(() =>
                    {
                        if (!IsVisible) return;
                        progress.Value++;
                    });
                } catch (Exception e)
                {
                    Log.log(e.Message, Log.WARN);
                }
            },
            (bool show) =>
            {
                try {
                    Dispatcher.Invoke(() =>
                    {
                        if (!IsVisible) return;
                        if (show) Show();
                        else Hide();
                    });
                } catch (Exception e)
                {
                    Log.log(e.Message, Log.WARN);
                }
            });
            if(Setting.Name.Equals(""))
            {
                MyIp.Content = rm.GetString("mypc") + ": " + internet.myIP;
            }
            else
            {
                MyIp.Content = Setting.Name;
            }
            if(Setting.AutoScanOnLaunch)
            {
                Ping();
            }
            ///Arguments Loading
            originated = cmds[0];
            foreach(string i in cmds)
            {
                Console.WriteLine(i);
            }
            bool isAutoStart = false;
            if (cmds.Length > 1)
            {
                fileOpened = cmds[1];
                if(Setting.AutoScanOnVisible)
                {
                    Ping();
                }
                if (cmds.Length > 2)
                {
                    for (int i = 2; i < cmds.Length; i++)
                    {
                        if (cmds[i].Equals("-debugging"))
                        {
                            debugging = true;
                        }
                        else if(cmds[i].Equals("-ExitOnDone"))
                        {
                            exitOnDone = true;
                        }
                        else if (cmds[i].Equals("-server"))
                        {
                            internet.ServerStart();
                        }
                        else if (cmds[i].Equals("-pingReceiver"))
                        {
                            Task.Run(() =>
                            {
                                internet.PingReceiver();
                            });
                        }
                        else if (cmds[i].Equals("-autoStart"))
                        {
                            isAutoStart = true;
                            if (!Setting.AutoStartOnBoot)
                            {
                                Application.Current.Shutdown();
                            }
                        }
                        else if (cmds[i].Equals("-DoNotExitOnClosing"))
                        {
                            dontExitOnClosing = false;
                        }
                        else if (cmds[i].Equals("-tray"))
                        {
                            NotifyIcon ni = new NotifyIcon();
                            ContextMenu tray = new ContextMenu();
                            MenuItem exit = new MenuItem
                            {
                                Index = 0,
                                Text = rm.GetString("exit")
                            };
                            exit.Click += delegate (object click, EventArgs e)
                            {
                                Environment.Exit(0);
                            };
                            MenuItem open = new MenuItem
                            {
                                Text = rm.GetString("open")
                            };
                            open.Click += delegate (object click, EventArgs e)
                            {
                                Show();
                                if (Setting.AutoScanOnVisible)
                                {
                                    Ping();
                                }
                            };
                            MenuItem setting = new MenuItem
                            {
                                Text = rm.GetString("settings")
                            };
                            setting.Click += delegate (object click, EventArgs e)
                            {
                                settingPage.Show();
                            };
                            open.Index = 0;
                            tray.MenuItems.Add(setting);
                            tray.MenuItems.Add(open);
                            tray.MenuItems.Add(exit);
                            ResourceManager resource = new ResourceManager("Pass.Resource", typeof(MainWindow).Assembly);
                            ni.Icon = new Icon(Directory.GetParent(originated).FullName+"/Resources/pass.ico");
                            ni.Visible = true;
                            ni.DoubleClick += delegate (object senders, EventArgs e)
                            {
                                Show();
                            };
                            ni.ContextMenu = tray;
                            ni.Text = "Pass";
                        }
                        else
                        {
                            Log.log(cmds[i] + " is not recognized as a valid argument", Log.WARN);
                        }
                    }
                }
            }
            else
            {
                fileOpened = null;
            }
            ///
            /*
            if (!regedEdited)
            {
                Regedit.rm = rm;
                Regedit.originated = originated;
                Regedit.checkEnvironment(false);
            }
            */
            q.Visibility = hide;
            w.Visibility = hide;
            e.Visibility = hide;
            r.Visibility = hide;
            t.Visibility = hide;
            y.Visibility = hide;
            u.Visibility = hide;
            lst.Visibility = hide;
            sup.setResourceManager(rm);
            debuger.SetResourceManager(rm);
            if(!isAutoStart)
            {
                rootPane.Visibility = show;
            }
            Show();
        }
        private int keptKeyDown = 0;
        private long lastKeyDown=0;
        private readonly Debuger debuger = new Debuger();
        private void KeyDownDetected(object sender, KeyEventArgs e)
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
        private void Refresh_Click(object sender, RoutedEventArgs ex)
        {
            refresh.IsEnabled = false;
            none.Visibility = Visibility.Hidden;
            finding.Visibility = Visibility.Visible;
            lst.Visibility = Visibility.Hidden;
            Ping();
        }
        private void Ping()
        {
            refresh.IsEnabled = false;
            Task.Run(() =>
            {
                internet.lastResponse.Clear();
                List<string> ip = internet.ScanLocalIp();
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
                }
                else
                {
                    lst.Dispatcher.Invoke(() =>
                    {
                        lst.Visibility = show;
                    });
                    int queue = 0;
                    IP = ip;
                    q.Dispatcher.Invoke(() =>
                    {
                        q.Visibility = hide; w.Visibility = hide; e.Visibility = hide; r.Visibility = hide; t.Visibility = hide;
                        y.Visibility = hide; u.Visibility = hide;
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
                debuger.SetReponse(internet.lastResponse);
                refresh.Dispatcher.Invoke(() =>
                {
                    refresh.IsEnabled = true;
                });
                return;
            });
        }

        private readonly Support sup = new Support();
        private void Sup2_Click(object sender, RoutedEventArgs e)
        {
            sup.go(Support.NONE_VISIBLE);
        }
        private void Sup1_Click(object sender, RoutedEventArgs e)
        {
            sup.go(Support.NONE_VISIBLE);
        }

        private void Killer(object sender, CancelEventArgs e)
        {
            if (debugging)
            {
                Application.Current.Shutdown();
            }
            else if(!dontExitOnClosing)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                Application.Current.Shutdown();
            }
        }

        private void Exec(int whoareu, Label label)
        {
            if(!Setting.Sharing)
            {
                MessageBox.Show(rm.GetString("sharingAlert"), "Pass", MessageBoxButtons.OK);
                return;
            }
            if (fileOpened == null || fileOpened.Equals("null"))
            {
                MessageBox.Show(rm.GetString("nosel"), "Pass");
                return;
            }
            if (Setting.AskBeforeShare)
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
                int result = internet.WannaSendTo(ip, fileOpened);
                Log.clientLog("Result: "+Internet.reason);
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
                if(exitOnDone)
                {
                    Environment.Exit(0);
                }
            });
        }

        private void Q1_Click(object sender, RoutedEventArgs e)
        {
            Exec(0, ip1);
        }

        private void Q2_Click(object sender, RoutedEventArgs e)
        {
            Exec(1, ip2);
        }

        private void Q3_Click(object sender, RoutedEventArgs e)
        {
            Exec(2, ip3);
        }

        private void Q4_Click(object sender, RoutedEventArgs e)
        {
            Exec(3, ip4);
        }

        private void Q5_Click(object sender, RoutedEventArgs e)
        {
            Exec(4, ip5);
        }

        private void Q6_Click(object sender, RoutedEventArgs e)
        {
            Exec(5, ip6);
        }

        private void Q7_Click(object sender, RoutedEventArgs e)
        {
            Exec(6, ip7);
        }
    }
}
