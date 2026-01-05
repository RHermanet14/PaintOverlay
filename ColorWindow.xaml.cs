using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintOverlay
{
    /// <summary>
    /// Interaction logic for ColorWindow.xaml
    /// </summary>
    public partial class ColorWindow : Window
    {
        private readonly MainWindow _main;
        private SolidColorBrush? CustomColor;
        private bool SavingCustomColor = false;

        public ColorWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            Set_Custom_Colors();
        }

        private void Set_Custom_Colors()
        {
            RInput.Text = GInput.Text = BInput.Text = "255";
        }

        private void Color_Selected(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == null) return;
            if (e.Source is System.Windows.Shapes.Rectangle rect)
            {
                _main.Color_Changed(rect);
                string temp = rect.Name;
                temp = temp.Replace('_', ' ');
                ColorName.Text = temp;
            }   
        }

        private void Color_Saved(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(RInput.Text, out byte RColor) && byte.TryParse(GInput.Text, out byte GColor) && byte.TryParse(BInput.Text, out byte BColor))
            {
                System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, RColor, GColor, BColor);
                CustomColor = new(color);
                SavingCustomColor = true;
            }
        }

        private void Color_Changed(object sender, TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox input)
            {
                if (int.TryParse(input.Text, out int size))
                {
                    if (size > 255)
                    {
                        size = 255;
                        input.Text = size.ToString();
                    }         
                    else if (size < 0)
                    {
                        size = 0;
                        input.Text = size.ToString();
                    }
                }
            }
        }

        private void Custom_Color_Selected(object sender, MouseButtonEventArgs e)
        {
            if (CustomColor == null)
                return;
            if (e.Source is System.Windows.Shapes.Rectangle rect)
            {
                if (SavingCustomColor == true)
                    rect.Fill = CustomColor;
                _main.Color_Changed(rect);
                string temp = rect.Name;
                temp = temp.Replace('_', ' ');
                ColorName.Text = temp;

            }
            SavingCustomColor = false;
        }
    }
}
