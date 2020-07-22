using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pass.WPF
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        private bool isToggled = false;
        private Thickness left = new Thickness(6, 6, 45, 6);
        private Thickness right = new Thickness(45, 6, 6, 6);
        private SolidColorBrush off = new SolidColorBrush(Color.FromRgb(66, 66, 66));
        private SolidColorBrush on = new SolidColorBrush(Color.FromRgb(40, 190, 40));
        public ToggleButton()
        {
            InitializeComponent();
        }

        public bool IsToggled {
            get
            {
                return isToggled;
            }
            set
            {
                isToggled = value;
                if (!isToggled)
                {
                    back.Fill = off;
                    swit.Margin = left;
                }
                else
                {
                    back.Fill = on;
                    swit.Margin = right;
                }
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isToggled = !isToggled;
            if(!isToggled)
            {
                back.Fill = off;
                swit.Margin = left;
            }
            else
            {
                back.Fill = on;
                swit.Margin = right;
            }
        }

        private void back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse_MouseLeftButtonDown(sender, e);
        }
    }
}
