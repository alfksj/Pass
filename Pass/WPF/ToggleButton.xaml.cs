using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pass.WPF
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        private bool IsToggled = false;
        private Thickness left = new Thickness(6, 6, 45, 6);
        private Thickness right = new Thickness(45, 6, 6, 6);
        public ToggleButton()
        {
            InitializeComponent();
        }

        public bool isToggled {
            get
            {
                return IsToggled;
            }
            set
            {
                IsToggled = value;
                if (IsToggled)
                {
                    swit.Margin = right;
                    back.Fill = Resources["Green"] as Brush;
                }
                else
                {
                    swit.Margin = left;
                    back.Fill = Resources["Gray"] as Brush;
                }
            }
        }
        private void back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isToggled = !isToggled;
        }
    }
}
