using PassLibrary.Box;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Windows;

namespace Pass
{
    /// <summary>
    /// Interaction logic for Debuger.xaml
    /// </summary>
    public partial class Debuger : Window
    {
        public Debuger()
        {
            InitializeComponent();
        }
        private ResourceManager rm;
        public void SetResourceManager(ResourceManager rm)
        {
            this.rm = rm;
        }
        public void AbortClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }
        public void SetReponse(List<Response> response)
        {
            List<Response> Items = new List<Response>();
            response.ForEach((cons) =>
            {
                Items.Add(new Response()
                {
                    IP = cons.IP,
                    response = rm.GetString(cons.response)
                });
            });
            respon.Dispatcher.Invoke(() =>
            {
                respon.ItemsSource = Items;
            });
        }
    }
}
